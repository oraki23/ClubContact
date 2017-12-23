/*------------------------------------------------------------------------------------

CONTRÔLEUR QUI GÈRE L'ENVOI DE CADEAUX (EMOTICONS) ENTRE LES MEMBRES

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Controllers
{
    public class EmoticonController : BaseController
    {
        ClubContactContext db = new ClubContactContext();

        // GET: Emoticon
        public ActionResult EnvoyerEmoticon(int noEmoticon, int noMembreReceveur)
        {
            try
            {
                Emoticon lemoticon = db.Emoticons.Where(e => e.noEmoticon == noEmoticon).FirstOrDefault();

                int noMembreCo;

                Membre leMembreConnectee = null;
                Membre leMembreReceveur = db.Membres.Where(m => m.noMembre == noMembreReceveur).FirstOrDefault();

                if (verifierSiCookieNoMembreExiste(out noMembreCo))
                {
                    leMembreConnectee = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();
                }

                if (lemoticon == null)
                {
                    throw new Exception("L'objet Emoticon est null.");
                }
                else if(leMembreConnectee == null)
                {
                    throw new Exception("Le membre connecté est null.");
                }
                else if(leMembreReceveur == null)
                {
                    throw new Exception("Le membre receveur est null.");
                }

                Gift leNouveauCadeau = new Gift();

                leNouveauCadeau.dateEnvoi = DateTime.Now;
                leNouveauCadeau.emoticonEnvoye = lemoticon;

                leNouveauCadeau.membreReceveur = leMembreReceveur;

                leNouveauCadeau.membreEnvoyeur = leMembreConnectee;
                leNouveauCadeau.vientDePremium = leMembreConnectee.premium;

                leNouveauCadeau.supprimeDeReceveur = false;

                db.Gifts.Add(leNouveauCadeau);

                db.SaveChanges();
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "noEmoticon", noEmoticon.ToString() },
                    { "noMembreReceveur", noMembreReceveur.ToString() }
                };

                throw LesUtilitaires.Utilitaires.templateException("(AJAX) EnvoyerEmoticon", "Emoticon", "Requête LINQ n'a pas fonctionnée ou update BD n'a pas fonctionnée.",
                    e, parametres);
            }
            

            return null;
        }

        public ActionResult SupprimerGift(int noGift, int noMembreReceveur, int noMembreEnvoyeur)
        {
            Gift leCadeauASupprimer = db.Gifts.Where(g => g.noGift == noGift && g.noMembreEnvoyeur == noMembreEnvoyeur && g.noMembreReceveur == noMembreReceveur).FirstOrDefault();

            Membre leMembreReceveur = db.Membres.Where(m => m.noMembre == noMembreReceveur).FirstOrDefault();

            if(leCadeauASupprimer != null)
            {
                //leMembreReceveur.listeCadeauxRecus.Remove(leCadeauASupprimer);

                leCadeauASupprimer.supprimeDeReceveur = true;

                db.SaveChanges();
            }
            return PartialView("~/Views/Emoticon/ToutesEmoticonRecuPartial.cshtml", leMembreReceveur);
        }
    }
}