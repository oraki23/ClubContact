/*------------------------------------------------------------------------------------

CONTRÔLEUR PERMETTANT LA GESTION DE TOUT CE QUI ATTRAIT À LA MESSAGERIE NOTAMMENT:
    ENVOIE DE MESSAGE
    RÉCEPTION DE MESSAGE
    LISTE DE MESSAGES
    AJOUT LISTE CONTACT
    RETIRER LISTE CONTACT
    GESTION LISTE CONTACT
    ..ETC

    À NOTER QUE TOUT CE QUI ATTRAIT AUX CADEAUX A SON PROPRE CONTRÔLLEUR (EMOTICONCONTROLLER)

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using ProjetSiteDeRencontre.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Controllers
{
    public class MessagerieController : BaseController
    {
        ClubContactContext db = new ClubContactContext();
        
        [NonAction]
        public void SetterViewDataListeContact(int noMembreCo)
        {
            Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).Include(m => m.listeContacts).FirstOrDefault();

            ViewData["listeContacts"] = leMembreCo.listeContacts.OrderBy(m => m.surnom).ToList();
        }

        // GET: Messagerie
        [HttpGet]
        public ActionResult Inbox(string categorie, int? page)
        {
            int nbItemsParPages = 20;

            ViewBag.Title = "Messagerie";

            int noMembreCo;
            if (!verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                return RedirectToAction("Home", "Home");
            }

            if (categorie == null)
            {
                categorie = "inbox";
            }

            IQueryable<Message> messagesMembreQuery = null;

            switch(categorie)
            {
                case "inbox":
                    messagesMembreQuery = db.Messages
                        .Where(m => m.noMembreReceveur == noMembreCo && !m.dansCorbeilleCoteReceveur && !m.supprimerCoteReceveur)
                        .OrderBy(l => l.lu).ThenByDescending(d => d.dateEnvoi);
                    ViewBag.boiteDeMessage = "inbox";
                    break;
                case "sent":
                    messagesMembreQuery = db.Messages
                        .Where(m => m.noMembreEnvoyeur == noMembreCo && !m.dansCorbeilleCoteEnvoyeur && !m.supprimerCoteEnvoyeur)
                        .OrderByDescending(d => d.dateEnvoi);
                    ViewBag.boiteDeMessage = "sent";
                    break;
                case "deleted":
                    messagesMembreQuery = db.Messages
                        .Where(m => (m.noMembreReceveur == noMembreCo && m.dansCorbeilleCoteReceveur && !m.supprimerCoteReceveur) || (m.noMembreEnvoyeur == noMembreCo && m.dansCorbeilleCoteEnvoyeur && !m.supprimerCoteEnvoyeur))
                        .OrderBy(l => l.lu).ThenByDescending(d => d.dateEnvoi);
                    ViewBag.boiteDeMessage = "deleted";
                    break;
            }

            GestionPagination(page, nbItemsParPages, messagesMembreQuery.Count());

            List<Message> messagesMembre = messagesMembreQuery
            .Skip((int)(ViewBag.currentPage - 1) * nbItemsParPages)
                .Take(nbItemsParPages)
                .Include(m => m.membreEnvoyeur)
                .ToList();

            SetterViewDataListeContact(noMembreCo);
            return View("Inbox", messagesMembre);
        }

        [HttpPost]
        public ActionResult Inbox(string categorie, int[] checker, string actionInbox, int page)
        {
            if(actionInbox == "repondre")
            {
                int noMessage = checker.Where(c => c != -1).FirstOrDefault();
                Message leMessage = db.Messages.Where(m => m.noMessage == noMessage).FirstOrDefault();

                if(leMessage != null)
                {
                    return RedirectToAction("Send", "Messagerie", new { id = leMessage.noMembreEnvoyeur, noMessageRepondu = noMessage });
                }
            }
            else if(actionInbox == "supprimer")
            {
                foreach(int noMessage in checker)
                {
                    if(noMessage != -1)
                    {
                        SupprimerMessage(noMessage);
                    }
                }

                return RedirectToAction("Inbox", "Messagerie", new { categorie = categorie, page = page });
            }
            else if(actionInbox == "marquerCommeLu")
            {
                if(categorie == "inbox")
                {
                    foreach (int noMessage in checker)
                    {
                        if(noMessage != -1)
                        {
                            MarquerMessageCommeLu(noMessage);
                        }
                    }
                    return RedirectToAction("Inbox", "Messagerie", new { categorie = categorie, page = page });
                }
            }
            else if(actionInbox == "recuperer")
            {
                if(categorie == "deleted")
                {
                    foreach (int noMessage in checker)
                    {
                        if(noMessage != -1)
                        {
                            RecupererMessage(noMessage);
                        }
                    }
                    return RedirectToAction("Inbox", "Messagerie", new { categorie = categorie, page = page });
                }
            }


            /*Le get Normal, n'est pas DRY, mais à revoir*/
            return RedirectToAction("Inbox", "Messagerie", new { categorie = categorie, page = page });
        }

        public ActionResult ToutSupprimerMessagesCorbeille()
        {
            int noMembreCo;
            if (!verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                return RedirectToAction("Home", "Home");
            }

            try
            {
                Membre leMembre = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                List<Message> messagesASupprimerCoteEnvoyeur = db.Messages.Where(m => m.noMembreEnvoyeur == leMembre.noMembre && m.dansCorbeilleCoteEnvoyeur == true).ToList();
                List<Message> messagesASupprimerCoteReceveur = db.Messages.Where(m => m.noMembreReceveur == leMembre.noMembre && m.dansCorbeilleCoteReceveur == true).ToList();
                messagesASupprimerCoteEnvoyeur.AddRange(messagesASupprimerCoteReceveur);

                //List<Message> messagesASupprimerDefinitivement = new List<Message>();

                foreach(Message m in messagesASupprimerCoteEnvoyeur)
                {
                    if(m.noMembreEnvoyeur == leMembre.noMembre)
                    {
                        m.supprimerCoteEnvoyeur = true;
                    }
                    if(m.noMembreReceveur == leMembre.noMembre)
                    {
                        m.supprimerCoteReceveur = true;
                    }

                    //if(m.supprimerCoteEnvoyeur && m.supprimerCoteReceveur)
                    //{
                    //    messagesASupprimerDefinitivement.Add(m);
                    //}
                }

                //db.Messages.RemoveRange(messagesASupprimerDefinitivement);

                db.SaveChanges();
            }
            catch(Exception e)
            {
                throw LesUtilitaires.Utilitaires.templateException("ToutSupprimerMessagesCorbeille", "Messagerie", "La suppression des messages n'a pas fonctionnée ou requête LINQ n'a pas fonctionnée.",
                    e);
            }

            return RedirectToAction("Inbox", new { categorie = "deleted" });
        }

        public ActionResult InboxMessage(int id)
        {
            int noMembreCo;
            if (!verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                return RedirectToAction("Home", "Home");
            }

            Message leMessage = db.Messages.Where(m => (m.noMembreReceveur == noMembreCo || m.noMembreEnvoyeur == noMembreCo) && m.noMessage == id).FirstOrDefault();

            if(leMessage.noMembreReceveur == noMembreCo)
            {
                leMessage.lu = true;
                try
                {
                    db.SaveChanges();
                }
                catch(Exception e)
                {
                    Dictionary<string, string> parametres = new Dictionary<string, string>()
                    {
                        { "id", id.ToString() }
                    };
                    throw LesUtilitaires.Utilitaires.templateException("InboxMessage", "Messagerie", "La sauvegarde du message pour le changement à Lu n'a pas fonctionné.",
                        e, parametres);
                }
            }

            SetterViewDataListeContact(noMembreCo);
            return View("Message", leMessage);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult MessageAdmin(int id, int noSignalement)
        {
            Message leMessage = db.Messages.Where(m => m.noMessage == id).FirstOrDefault();

            ViewBag.ViewMessageAdminOnly = true;
            ViewBag.signalementLie = db.Signalements.Where(m => m.noSignalement == noSignalement).FirstOrDefault();

            return View("Message", leMessage);
        }

        [NonAction]
        public bool MarquerMessageCommeLu(int noMessage)
        {
            bool success = true;
            try
            {
                Message message = db.Messages.Where(m => m.noMessage == noMessage).FirstOrDefault();

                int noMembreCo;
                if (!verifierSiCookieNoMembreExiste(out noMembreCo))
                {
                    throw new Exception("Le Cookie noMembre n'existe pas.");
                }
                Membre leMembreConnectee = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                if (message.noMembreReceveur == leMembreConnectee.noMembre)
                {
                    message.lu = true;
                }
                else
                {
                    success = false;
                }

                db.SaveChanges();

                return success;
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "noMessage", noMessage.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("MarquerMessageCommeLu", "Messagerie", "La lecture du message n'a pas fonctionnée.",
                    e, parametres);
            }
        }

        [NonAction]
        public bool RecupererMessage(int noMessage)
        {
            bool succes = true;

            try
            {
                Message message = db.Messages.Where(m => m.noMessage == noMessage).FirstOrDefault();

                int noMembreCo;
                if (!verifierSiCookieNoMembreExiste(out noMembreCo))
                {
                    throw new Exception("Le Cookie noMembre n'existe pas.");
                }
                Membre leMembreConnectee = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                if (message.noMembreReceveur == message.noMembreEnvoyeur && message.noMembreEnvoyeur == leMembreConnectee.noMembre)
                {
                    message.dansCorbeilleCoteEnvoyeur = false;
                    message.dansCorbeilleCoteReceveur = false;
                }
                else if (message.noMembreEnvoyeur == leMembreConnectee.noMembre)
                {
                    message.dansCorbeilleCoteEnvoyeur = false;
                }
                else if (message.noMembreReceveur == leMembreConnectee.noMembre)
                {
                    message.dansCorbeilleCoteReceveur = false;
                }
                else
                {
                    succes = false;
                }

                db.SaveChanges();

                return succes;
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "noMessage", noMessage.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("RecupererMessage", "Messagerie", "La récupération du message n'a pas fonctionnée.",
                    e, parametres, "get");
            }
        }

        [NonAction]
        public string SupprimerMessage(int noMessage)
        {
            try
            {
                Message message = db.Messages.Where(m => m.noMessage == noMessage).FirstOrDefault();

                int noMembreCo;
                if (!verifierSiCookieNoMembreExiste(out noMembreCo))
                {
                    throw new Exception("Le Cookie noMembre n'existe pas.");
                }
                Membre leMembreConnectee = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                string categorie = "inbox";

                if (message.noMembreReceveur == message.noMembreEnvoyeur && message.noMembreEnvoyeur == leMembreConnectee.noMembre)
                {
                    if (message.dansCorbeilleCoteEnvoyeur && message.dansCorbeilleCoteReceveur)
                    {
                        message.supprimerCoteEnvoyeur = true;
                        message.supprimerCoteReceveur = true;

                        categorie = "deleted";
                    }

                    //Ici, il s'est envoyé un message à lui-meme et le supprime
                    message.dansCorbeilleCoteEnvoyeur = true;
                    message.dansCorbeilleCoteReceveur = true;
                }
                else if (message.noMembreEnvoyeur == leMembreConnectee.noMembre)
                {
                    //Si le message est déjà dans la corbeille, on va le supprimer.
                    if (message.dansCorbeilleCoteEnvoyeur)
                    {
                        message.supprimerCoteEnvoyeur = true;

                        categorie = "deleted";
                    }
                    else
                    {
                        categorie = "sent";
                    }

                    message.dansCorbeilleCoteEnvoyeur = true;
                }
                else if (message.noMembreReceveur == leMembreConnectee.noMembre)
                {
                    if (message.dansCorbeilleCoteReceveur)
                    {
                        message.supprimerCoteReceveur = true;
                    }
                    else
                    {
                        categorie = "inbox";
                    }

                    message.dansCorbeilleCoteReceveur = true;
                }

                db.SaveChanges();

                return categorie;
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "noMessage", noMessage.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("DeleteMessage", "Messagerie", "La suppression du message n'a pas fonctionnée.",
                    e, parametres, "get");
            }
        }

        public ActionResult EnvoyerCorbeilleMessage(Message message)
        {
            if(message == null)
            {
                if(TempData["leMessage"] != null)
                {
                    message = TempData["leMessage"] as Message;
                }
            }

            //à Modifier
            try
            {
                message = db.Messages.Where(m => m.noMessage == message.noMessage).FirstOrDefault();

                string categorie = SupprimerMessage(message.noMessage);

                return RedirectToAction("Inbox", "Messagerie", new { categorie = categorie });
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "noMessage", message.noMessage.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("DeleteMessage", "Messagerie", "La suppression du message n'a pas fonctionnée.",
                    e, parametres, "get");
            }
        }

        /*public ActionResult SignalerMessage(int? id)
        {
            int noMembreCo;
           
            try
            {
                if(!verifierSiCookieNoMembreExiste(out noMembreCo))
                {
                    throw new Exception("Le cookie noMembre n'existe pas.");
                }
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "id", id.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("SignalerMessage", "Messagerie", "Le cookie noMembre n'existe pas.",
                    e, parametres);
            }

            Message message;

            try
            {
                Membre leMembre = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                message = db.Messages.Where(m => m.noMessage == id && (m.noMembreEnvoyeur == leMembre.noMembre || m.noMembreReceveur == leMembre.noMembre)).FirstOrDefault();

                message.messageSignale = true;

                db.SaveChanges();
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "id", id.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("SignalerMessage", "Messagerie", "La requête LINQ n'a pas fonctionnée ou la mise à jour de la BD non plus.",
                    e, parametres);
            }

            return RedirectToAction("EnvoyerCorbeilleMessage", "Messagerie", new { noMessage = message.noMessage });
        }*/

        public ActionResult AjouterOuSupprimerContacts(int noMessageBase, int noMembreCo, int noMembreAAjouterOuSupprimer, bool ajouter)
        {
            Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();
            Membre leMembreAAjouterOuSupprimer = db.Membres.Where(m => m.noMembre == noMembreAAjouterOuSupprimer).FirstOrDefault();

            if(ajouter)
            {
                leMembreCo.listeContacts.Add(leMembreAAjouterOuSupprimer);
            }
            else
            {
                if(leMembreCo.listeContacts.Contains(leMembreAAjouterOuSupprimer))
                {
                    leMembreCo.listeContacts.Remove(leMembreAAjouterOuSupprimer);
                }
            }

            db.SaveChanges();

            return RedirectToAction("InboxMessage", "Messagerie", new { id = noMessageBase });
        }

        public int SupprimerContactsAJAX(int noMembreCo, int noMembreASupprimer)
        {
            Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();
            Membre leMembreAAjouterOuSupprimer = db.Membres.Where(m => m.noMembre == noMembreASupprimer).FirstOrDefault();

            if (leMembreCo.listeContacts.Contains(leMembreAAjouterOuSupprimer))
            {
                leMembreCo.listeContacts.Remove(leMembreAAjouterOuSupprimer);
            }

            db.SaveChanges();

            return noMembreASupprimer;
        }

        [HttpGet]
        public ActionResult Send(int? id, int? noMessageRepondu, int? noSignalementLie)
        {
            ViewBag.Title = "Envoyer un message";

            if(id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            int noMembreCo;
            verifierSiCookieNoMembreExiste(out noMembreCo);

            bool messageVientDeAdmin = false;
            if (!verifierSiCookieNoMembreExiste(out noMembreCo))
            {
                messageVientDeAdmin = true;
            }

            Membre leMembreConnecte = null;
            Membre leMembreReceveur;
            try
            {
                if(!messageVientDeAdmin)
                {
                    leMembreConnecte = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();
                }
                leMembreReceveur = db.Membres.Where(m => m.noMembre == id).FirstOrDefault();
            }
            catch(Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>()
                {
                    { "id", id.ToString() }
                };
                throw LesUtilitaires.Utilitaires.templateException("Send", "Messagerie", "Requête LINQ n'a pas fonctionnée.",
                    e, parametres, "get");
            }

            Message nouveauMessage = new Message();

            nouveauMessage.membreEnvoyeur = leMembreConnecte;
            nouveauMessage.membreReceveur = leMembreReceveur;

            nouveauMessage.noMembreEnvoyeur = !messageVientDeAdmin ? leMembreConnecte.noMembre : (int?)null;
            nouveauMessage.noMembreReceveur = leMembreReceveur.noMembre;

            if(noMessageRepondu != null)
            {
                Message leMessageRepondu = db.Messages.Where(m => m.noMessage == noMessageRepondu).FirstOrDefault();

                if(leMessageRepondu != null)
                {
                    nouveauMessage.sujetMessage = "RE: " + leMessageRepondu.sujetMessage;
                    nouveauMessage.contenuMessage = 
                        "\n\n-----------------\n" +
                        "Le " + leMessageRepondu.dateEnvoi.ToLongDateString() + " à " + leMessageRepondu.dateEnvoi.ToShortTimeString() + ", " + leMessageRepondu.membreEnvoyeur.surnom + " a écrit:\n\n" + 
                        leMessageRepondu.contenuMessage;
                }
            }

            ViewBag.ActionActuelle = "send";

            if(!messageVientDeAdmin)
            {
                SetterViewDataListeContact(noMembreCo);
            }
            else
            {
                ViewBag.noSignalementLie = noSignalementLie;
            }

            return View("Send", nouveauMessage);
        }

        [HttpPost]
        public ActionResult Send(Message message, int? noSignalementLie)
        {
            message.dateEnvoi = DateTime.Now;
            message.lu = false;

            message.dansCorbeilleCoteEnvoyeur = false;
            message.dansCorbeilleCoteReceveur = false;

            message.supprimerCoteEnvoyeur = false;
            message.supprimerCoteReceveur = false;

            bool messageVientDeAdmin = true;
            //Si c'est pas l'admin qui l'envoie, on s'en fout du principe de liste noire
            if(message.noMembreEnvoyeur != null)
            {
                messageVientDeAdmin = false;

                message.membreEnvoyeur = db.Membres.Where(m => m.noMembre == message.noMembreEnvoyeur).FirstOrDefault();
                message.membreReceveur = db.Membres.Where(m => m.noMembre == message.noMembreReceveur).FirstOrDefault();

                //Si le membre qui va recevoir le message a le membres qui l'a envoyé dans sa liste noire, alors le message sera automatiquement supprimé de son côté
                //Du côté envoyeur, il va voir comme s'il l'a envoyé
                if (message.membreReceveur.listeNoire.Contains(message.membreEnvoyeur))
                {
                    message.dansCorbeilleCoteReceveur = true;
                    message.supprimerCoteReceveur = true;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Messages.Add(message);

                    db.SaveChanges();

                    if(!messageVientDeAdmin)
                    {
                        return RedirectToAction("Inbox", "Messagerie");
                    }
                    else
                    {
                        if (noSignalementLie != null)
                        {
                            CompteAdmin adminConnectee = db.CompteAdmins.Where(m => m.nomCompte == User.Identity.Name).FirstOrDefault();

                            ActionTraitement actionTraitement = db.ActionTraitements.Where(m => m.nomActionTraitement == "Avertissement envoyé").FirstOrDefault();

                            Signalement leSignalement = db.Signalements.Where(m => m.noSignalement == noSignalementLie).FirstOrDefault();
                            leSignalement.dateSuiviNecessaire = DateTime.Now.AddDays(7);
                            leSignalement.etatSignalementActuel = db.EtatSignalements.Where(m => m.nomEtatSignalement == "En suivi").FirstOrDefault();
                            leSignalement.adminQuiTraite = adminConnectee;

                            TraitementSignalement nouveauTraitementFait = new TraitementSignalement();
                            nouveauTraitementFait.dateTraitementSignalement = DateTime.Now;
                            nouveauTraitementFait.noSignalementLie = leSignalement.noSignalement;
                            nouveauTraitementFait.signalementLie = leSignalement;
                            nouveauTraitementFait.compteAdminTraiteur = adminConnectee;
                            nouveauTraitementFait.noCompteAdminTraiteur = adminConnectee.noCompteAdmin;
                            nouveauTraitementFait.messageLie = message;
                            //nouveauTraitementFait.noMessageLie = message.noMessage;
                            nouveauTraitementFait.actionTraitement = actionTraitement;
                            nouveauTraitementFait.noActionTraitement = actionTraitement.noActionTraitement;

                            db.TraitementSignalements.Add(nouveauTraitementFait);

                            db.SaveChanges();
                        }

                        return RedirectToAction("Gestion", "Admin", new { tab = 2 });
                    }
                }
                catch(Exception e)
                {
                    Dictionary<string, string> parametres = new Dictionary<string, string>()
                    {
                        { "messageContenu", message.contenuMessage.ToString() },
                        { "sujetMessage", message.sujetMessage.ToString() }
                    };
                    throw LesUtilitaires.Utilitaires.templateException("Send", "Messagerie", "Ajout d'un message à la BD n'a pas fonctionné.",
                        e, parametres, "post");
                }
                
            }

            if(!messageVientDeAdmin)
            {
                int noMembreCo;
                verifierSiCookieNoMembreExiste(out noMembreCo);

                SetterViewDataListeContact(noMembreCo);
            }
            return View("Send", message);
        }

        [HttpGet]
        public ActionResult SendGlobal()
        {
            ViewBag.envoyerA = new List<SelectListItem>
            {
                new SelectListItem() { Text =  "Premiums", Value = true.ToString() },
                new SelectListItem() { Text = "Gratuits", Value = false.ToString() }
            };

            MessageGlobalViewModel messageGlobal = new MessageGlobalViewModel();

            return View("SendGlobal", messageGlobal);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult SendGlobal(MessageGlobalViewModel messageGlobal)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    EnvoyerMessageGlobal(messageGlobal);
                }
                catch (Exception e)
                {
                    Elmah.ErrorSignal.FromCurrentContext().Raise(e);
                    TempData["Error"] = "Une erreur s'est produite lors de votre envoie des messages, veuillez réessayer.";
                    TempData.Keep();

                    ViewBag.envoyerA = new List<SelectListItem>
                    {
                        new SelectListItem() { Text =  "Premiums", Value = true.ToString() },
                        new SelectListItem() { Text = "Gratuits", Value = false.ToString() }
                    };

                    return View("SendGlobal", messageGlobal);
                }
            }
            else
            {
                ViewBag.envoyerA = new List<SelectListItem>
                {
                    new SelectListItem() { Text =  "Premiums", Value = true.ToString() },
                    new SelectListItem() { Text = "Gratuits", Value = false.ToString() }
                };

                return View("SendGlobal", messageGlobal);
            }
            return RedirectToAction("Gestion", "Admin");
        }

        public async Task EnvoyerMessageGlobal(MessageGlobalViewModel messageGlobal)
        {
            List<int> lesNoMembres = db.Membres.Where(m =>
                                     ((messageGlobal.envoyerUniquementAPremium == null ? true : false) || m.premium == messageGlobal.envoyerUniquementAPremium) &&
                                     (m.compteSupprimeParAdmin == null && m.dateSuppressionDuCompte == null)
                                    )
                                    .Select(m => m.noMembre)
                                    .ToList();

            foreach (int i in lesNoMembres)
            {
                Message leMessage = new Message();
                leMessage.sujetMessage = messageGlobal.sujetMessage;
                leMessage.contenuMessage = messageGlobal.contenuMessage;
                leMessage.noMembreEnvoyeur = null;
                leMessage.noMembreReceveur = i;
                leMessage.lu = false;
                leMessage.dateEnvoi = DateTime.Now;
                leMessage.dansCorbeilleCoteEnvoyeur = false;
                leMessage.dansCorbeilleCoteReceveur = false;
                leMessage.supprimerCoteEnvoyeur = false;
                leMessage.supprimerCoteReceveur = false;

                db.Messages.Add(leMessage);
            }

            await db.SaveChangesAsync();
        }

        public int CompterNombreDeMessages(int noMembre, string boite)
        {
            if(boite == "deleted")
            {
                List<Message> messagesMembreCorbeilleEnvoyer = db.Messages.Where(m => m.noMembreReceveur == noMembre && m.dansCorbeilleCoteReceveur && !m.supprimerCoteReceveur).ToList();
                List<Message> messagesMembreCorbeilleReceveur = db.Messages.Where(m => m.noMembreEnvoyeur == noMembre && m.dansCorbeilleCoteEnvoyeur && !m.supprimerCoteEnvoyeur).ToList();
                messagesMembreCorbeilleEnvoyer.AddRange(messagesMembreCorbeilleReceveur);
                return messagesMembreCorbeilleEnvoyer.Count();
            }
            else if(boite == "inbox")
            {
                return db.Messages.Where(m => m.noMembreReceveur == noMembre && !m.dansCorbeilleCoteReceveur && !m.supprimerCoteReceveur && m.lu == false).Count();
            }
            return 0;
        }
    }
}