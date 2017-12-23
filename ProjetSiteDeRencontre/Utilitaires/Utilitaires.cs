/*------------------------------------------------------------------------------------

CLASSE UTILITAIRE AYANT PLEIN DE DIFFÉRENTES FONCTIONS UTILITAIRES QUI SONT RÉUTILISÉS
TOUT AU LONG DU SITE WEB

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.LesUtilitaires
{
    public static partial class Utilitaires
    {
        /* TEMPLATE POUR LES ERREURS (AVEC PARAMÈTRES)
         
        Dictionary<string, string> parametres = new Dictionary<string, string>() {
            { "noMembre", membre.noMembre.ToString()  }
        };
        throw Utilitaires.templateException("RemplirListesDeroulantesEtPreselectionnerHobbies", "Membres", "LRequête LINQ n'a pas fonctionnée ou la BD est inaccessible.", e, parametres, "post");
        
            */
        public static Exception templateException(string nomAction, string nomControleur, string erreurPotentielle = null, Exception innerException = null, Dictionary<string, string> parametresUtiles = null, string getOuPost = "Aucune")
        {
            string messageErreur = "Erreur: Action: " + nomAction;

            if(getOuPost.ToLower() == "get")
            {
                messageErreur += " (GET)";
            }
            else if(getOuPost.ToLower() == "post")
            {
                messageErreur += " (POST)";
            }

            messageErreur += ", Contrôleur: " + nomControleur + "; \n";

            if(erreurPotentielle != null)
            {
                messageErreur += "Erreur potentielle: " + erreurPotentielle + "; \n";
            }

            if(parametresUtiles != null)
            {
                messageErreur += "Paramètres:\n";
                foreach(KeyValuePair<string, string> parametre in parametresUtiles)
                {
                    messageErreur += "Variable: " + parametre.Key + " : " + parametre.Value + " \n";
                }
            }
            return new Exception(messageErreur, innerException);
        }

        /// <summary>
        /// Méthode qui effectue une transaction avec Paypal
        /// </summary>
        /// <param name="qui">Notes spéciales</param>
        /// <param name="ccNumber">Numéro de carte de crédit</param>
        /// <param name="ccType">Type de CC</param>
        /// <param name="expireDate">Date d'expiration</param>
        /// <param name="cvvNum">Numéro de sécurité</param>
        /// <param name="amount">Montant</param>
        /// <param name="firstName">Premier nom</param>
        /// <param name="lastName">Nom de famille</param>
        /// <param name="street">adresse</param>
        /// <param name="city">ville</param>
        /// <param name="state">province/état</param>
        /// <param name="zip">Code postal</param>
        /// <returns>Résultat de l'opération dans un String</returns>
        public static String doTransaction(string qui, string ccNumber, string ccType, string expireDate, string cvvNum, string amount, string firstName, string lastName, string street, string city, string state, string zip)
        {
            String retour = "";

            //API INFORMATIONS (3) 
            // https://www.sandbox.paypal.com/signin/  
            // REGARDER payment received
            // PARLER DE IPN

            String strUsername = "arassart-facilitator_api1.gmail.com";                            // nom du compte marchand dans le sandbox, payment pro activé
            String strPassword = "1397566287";                                                // password du ccompte marchand
            String strSignature = "AC9Q08PxViawtHP02THEb8XxIkxkA4xEwrqh4Z1Or4uoBpmX9BLd7IhR"; // clé d'authentification compte marchand

            string strCredentials = "CUSTOM=" + qui + "&USER=" + strUsername + "&PWD=" + strPassword + "&SIGNATURE=" + strSignature;
            string strNVPSandboxServer = "https://api-3t.sandbox.paypal.com/nvp";
            string strAPIVersion = "2.3";


            dynamic strNVP = strCredentials + "&METHOD=DoDirectPayment" + "&CREDITCARDTYPE=" + ccType + "&ACCT=" + ccNumber + "&EXPDATE=" + expireDate + "&CVV2=" + cvvNum + "&AMT=" + amount + "&CURRENCYCODE=CAD" + "&FIRSTNAME=" + firstName + "&LASTNAME=" + lastName + "&STREET=" + street + "&CITY=" + city + "&STATE=" + state + "&ZIP=" + zip + "&COUNTRYCODE=CA" + "&PAYMENTACTION=Sale" + "&VERSION=" + strAPIVersion;
            try
            {
                //Cree la requête
                System.Net.HttpWebRequest wrWebRequest = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(strNVPSandboxServer);
                wrWebRequest.Method = "POST";
                System.IO.StreamWriter requestWriter = new System.IO.StreamWriter(wrWebRequest.GetRequestStream());
                requestWriter.Write(strNVP);
                requestWriter.Close();
                //Obtient la réponse
                System.Net.HttpWebResponse hwrWebResponse = (System.Net.HttpWebResponse)wrWebRequest.GetResponse();
                dynamic responseReader = new System.IO.StreamReader(wrWebRequest.GetResponse().GetResponseStream());
                // Lit la réponse
                string dataReturned = responseReader.ReadToEnd();
                responseReader.Close();
                string result = HttpUtility.UrlDecode(dataReturned);
                string[] arrResult = result.Split('&');
                Hashtable htResponse = new Hashtable();
                string[] arrayReturned = null;
                foreach (string item in arrResult)
                {
                    arrayReturned = item.Split('=');
                    htResponse.Add(arrayReturned[0], arrayReturned[1]);
                }

                string strAck = htResponse["ACK"].ToString();
                //AFFICHE LA RÉPONSE

                if (strAck == "Success" || strAck == "SuccessWithWarning")
                {
                    string strAmt = htResponse["AMT"].ToString();
                    string strCcy = htResponse["CURRENCYCODE"].ToString();
                    string strTransactionID = htResponse["TRANSACTIONID"].ToString();
                    foreach (DictionaryEntry i in htResponse)
                    {
                        retour += i.Key + ": " + i.Value + "<br />";
                    }
                    //on retourne le message de confirmation
                    retour = "true";
                    //retour = "Merci pour votre commande de : $" + strAmt + " " + strCcy + ", celle-ci a bien été traitée.";

                }
                else
                {
                    //Dim strErr As String = "Error: " & htResponse("L_LONGMESSAGE0").ToString()
                    //Dim strErrcode As String = "Error code: " & htResponse("L_ERRORCODE0").ToString()

                    //Response.Write(strErr & "&lt;br /&gt;" & strErrcode)</span>
                    foreach (DictionaryEntry i in htResponse)
                    {
                        retour += i.Key + ": " + i.Value + "<br />";
                    }

                    //retourne les éléments des erreurs importantes
                    retour = "Error: " + htResponse["L_LONGMESSAGE0"].ToString() + "<br/>" + "Error code: " + htResponse["L_ERRORCODE0"].ToString();
                }

            }
            catch (Exception ex)
            {
                retour = ex.ToString();
            }
            return retour;
        }
        public static void trouverLatLng(string nomVille, string nomProvince, out decimal lat, out decimal lng)
        {
            lat = 0;
            lng = 0;

            string url = "http://maps.googleapis.com/maps/api/geocode/json?sensor=true&address=";
            dynamic googleResults = new Uri(url + nomVille + "," + nomProvince + ", Canada").GetDynamicJsonObject();
            foreach (var result in googleResults.results)
            {
                lat = (decimal)result.geometry.location.lat;
                lng = (decimal)result.geometry.location.lng;
            }
        }

        public static decimal calcDistance(decimal latA, decimal longA, decimal latB, decimal longB)
        {
            double theDistance = (Math.Sin(DegreesToRadians(latA)) *
                    Math.Sin(DegreesToRadians(latB)) +
                    Math.Cos(DegreesToRadians(latA)) *
                    Math.Cos(DegreesToRadians(latB)) *
                    Math.Cos(DegreesToRadians(longA - longB)));

            return Convert.ToDecimal((RadiansToDegrees(Math.Acos(theDistance)))) * 69.09M * 1.6093M;
        }

        public static double DegreesToRadians(decimal angle)
        {
            return (Math.PI / 180) * (double)angle;
        }

        private static double RadiansToDegrees(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static int CalculerPourcentageMembre(Membre membre)
        {
            int pourcentageCompleterProfil = 25;
            if (membre.nbEnfants != null)
            {
                pourcentageCompleterProfil += 3;
            }
            if(membre.nbAnimaux != null)
            {
                pourcentageCompleterProfil += 2;
            }
            if(membre.description != null)
            {
                pourcentageCompleterProfil += 10;
            }
            if(membre.noTaille != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if (membre.noSilhouette != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if (membre.noCheveuxCouleur != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if (membre.noYeuxCouleur != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if (membre.noReligion != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if (membre.noOrigine != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if (membre.fumeur != null)
            {
                pourcentageCompleterProfil += 5;
            }
            if(membre.listePhotosMembres != null)
            {
                if(membre.listePhotosMembres.Count > 5)
                {
                    pourcentageCompleterProfil += 25;
                }
                else
                {
                    pourcentageCompleterProfil += (membre.listePhotosMembres.Count * 5);
                }
            }

            try
            {
                int oldPourcentageCompleterProfil = pourcentageCompleterProfil;

                if (pourcentageCompleterProfil > 100)
                {
                    pourcentageCompleterProfil = 100;

                    throw new Exception("Méthode: CalculerPourcentageMembre, Classe: Utilitaires ; Erreur potentielle: Le pourcentage de complétion du profil dépasse 100%! ; Paramètres:"
                        + "\nnoMembre: " + membre.noMembre
                        + "\nnomMembre: " + membre.nom
                        + "\nprenomMembre: " + membre.prenom
                        + "\ncourrielMembre: " + membre.courriel
                        + "\npourcentageCompleterProfil: " + pourcentageCompleterProfil);
                }
                else if(pourcentageCompleterProfil < 0)
                {
                    pourcentageCompleterProfil = 0;

                    throw new Exception("Méthode: CalculerPourcentageMembre, Classe: Utilitaires ; Erreur potentielle: Le pourcentage de complétion du profil est en dessous de 0%! ; Paramètres:"
                        + "\nnoMembre: " + membre.noMembre
                        + "\nnomMembre: " + membre.nom
                        + "\nprenomMembre: " + membre.prenom
                        + "\ncourrielMembre: " + membre.courriel
                        + "\npourcentageCompleterProfil: " + pourcentageCompleterProfil);
                }
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            return pourcentageCompleterProfil;
        }

        public static bool hasUpperCase(string str)
        {
            return str.ToLower() != str;
        }
        public static bool hasLowerCase(string str)
        {
            return str.ToUpper() != str;
        }
        public static bool hasNumbers(string str)
        {
            return str.Any(char.IsDigit);
        }
        public static bool hasLetters(string str)
        {
            return str.Any(char.IsLetter);
        }

        public static int CalculerAge(DateTime dateNaissance)
        {
            int age = 0;
            age = DateTime.Now.Year - dateNaissance.Year;
            if (DateTime.Now.DayOfYear < dateNaissance.DayOfYear)
                age = age - 1;

            return age;
        }

        public static void envoieCourriel(string sujet, string message, string destinataire, bool async = false)
        {
            //A changé l'adresse par défaut!!!!
            System.Net.Mail.MailMessage email = new System.Net.Mail.MailMessage("projet2017@cegepgranby.qc.ca", destinataire/*"ABCguy58@hotmail.com"*/);
            string statut = "";
            string erreur = "";

            email.Subject = sujet;
            email.IsBodyHtml = true;
            email.Body = message;
            email.BodyEncoding = System.Text.Encoding.UTF8;

            System.Net.Mail.SmtpClient mailObj = null;

            try
            {
                mailObj = new System.Net.Mail.SmtpClient();

                mailObj.Credentials = new NetworkCredential("projet2017ClubContact@gmail.com", "chat4178");
                mailObj.Port = 587;
                mailObj.Host = "smtp.gmail.com";
                mailObj.EnableSsl = true;

                
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }

            try
            {
                mailObj.Send(email);
                //A réussi à envoyé
            }
            catch (Exception e)
            {
                Elmah.ErrorSignal.FromCurrentContext().Raise(e);
            }
        }

        public static string RenderRazorViewToString(Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}

/*List<long> datesMilliseconds = new List<long>();

                foreach(DateTime d in lesDatesOuIlYADesActivites)
                {
                    datesMilliseconds.Add((long)(d - new DateTime(1970, 1, 1)).TotalMilliseconds);
                }*/
//List<Photo> photos = db.Photos.Where(p => p.noMembre == acti.noMembre).ToList();

//db.Visites.RemoveRange(visites);

//db.Photos.RemoveRange(photos);

//db.Activites.RemoveRange(activites);
//for (int i = 0; i < acti.listeActivitesOrganises.Count; i++)
//{
//    acti.listeActivitesOrganises.Remove(acti.listeActivitesOrganises[i]);
//}
////Retire tous les hobbies avant de supprimer
//for (int i = 0; i < acti.listeHobbies.Count; i++)
//{
//    acti.listeHobbies.Remove(acti.listeHobbies[i]);
//}

//for (int i = 0; i < acti.listeRaisonsSurSite.Count; i++)
//{
//    acti.listeRaisonsSurSite.Remove(acti.listeRaisonsSurSite[i]);
//}

//On archive les informations importantes du membre dans une autre table à des fins de statistiques
//MembreArchive membreArchive = new MembreArchive()
//{
//    dateSuppressionDuCompte = DateTime.Now,
//    nom = acti.nom,
//    prenom = acti.prenom,
//    surnom = acti.surnom,
//    courriel = acti.courriel,
//    dateNaissance = acti.dateNaissance,
//    homme = acti.homme,
//    nbDeSignalement = acti.nbDeSignalement,
//    premium = acti.premium,
//    province = acti.province,
//    ville = acti.ville
//};
//db.MembreArchives.Add(membreArchive);
