/*------------------------------------------------------------------------------------

CLASSE UTILITAIRE POUR TOUT CE QUI ATTRAIT AUX ACTIVITÉS, CONTENANT LES FONCTIONS UTILES 
POUR CE QUI TOUCHE LES ACTIVITÉS

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
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.LesUtilitaires
{
    public static partial class Utilitaires
    {
        public static string ParticiperActivite(int noActivite, int noMembre)
        {
            try
            {
                ClubContactContext db = new ClubContactContext();

                string etaitDejaParticipant;

                Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembre).FirstOrDefault();
                Activite lActivite = db.Activites.Where(a => a.noActivite == noActivite).Include(a => a.membresParticipants).FirstOrDefault();

                if (lActivite.membresParticipants.Contains(leMembreCo))
                {
                    lActivite.membresParticipants.Remove(leMembreCo);
                    etaitDejaParticipant = "True";
                }
                else
                {
                    lActivite.membresParticipants.Add(leMembreCo);
                    etaitDejaParticipant = "False";
                }

                db.SaveChanges();

                return etaitDejaParticipant;
            }
            catch (Exception e)
            {
                Dictionary<string, string> parametres = new Dictionary<string, string>() {
                        { "noActivite", noActivite.ToString() },
                        { "noMembre", noMembre.ToString() }
                    };
                throw Utilitaires.templateException("ParticiperActivite", "Classe Utilitaires.js", "Requête LINQ n'a pas fonctionnée.", e, parametres);
            }
        }

        public static string AnnulerActivite(int noActivite, int? noMembreCo, out Activite activite)
        {
            ClubContactContext db = new ClubContactContext();

            activite = db.Activites.Where(p => p.noActivite == noActivite).Include(m => m.listeAvisActivite).Include(m => m.listePhotosActivites).Include(m => m.membresParticipants).FirstOrDefault();

            if (activite == null)
            {
                return "Activité non trouvée.";
            }

            if (noMembreCo != null)
            {
                if (activite.noMembreOrganisateur != noMembreCo)
                {
                    return "Interdit";
                }
            }

            try
            {
                activite.annulee = true;

                db.SaveChanges();

                return "Reussi";
            }
            catch (Exception e)
            {
                throw Utilitaires.templateException("annulerActivite", "Objet Utilitaires.cs", "Suppression de l'activité n'as pas fonctionné.", e);
            }
        }

        public static IQueryable<Activite> RechercheActiviteAvecCriteres(int noMembreCo, RechercheActiviteViewModel rechercheActivite)
        {
            ClubContactContext db = new ClubContactContext();

            IQueryable<Activite> listActiviteTrouves = db.Activites;

            if (noMembreCo != -1)
            {
                Membre leMembreCo = db.Membres.Where(m => m.noMembre == noMembreCo).FirstOrDefault();

                if (leMembreCo != null)
                {
                    //On trouve uniquement les activités compatibles.
                    listActiviteTrouves = listActiviteTrouves
                        .Where(a => //Si le membre organisateur est le membre connecté, on affiche l'activité...
                                    (a.noMembreOrganisateur == leMembreCo.noMembre) ? true : 
                                    (
                                        ((a.hommeSeulement == null ? true : false) || (a.hommeSeulement == leMembreCo.homme)) &&
                                        ((a.ageMax == null ? true : false) || leMembreCo.age <= a.ageMax) &&
                                        ((a.ageMin == null ? true : false) || leMembreCo.age >= a.ageMin)
                                    )
                        );

                    //Critères Communs
                    listActiviteTrouves = listActiviteTrouves
                        .Where(a => //Activites que j'organise
                                    ((rechercheActivite.organiseParMoi == false ? true : (a.membreOrganisateur.noMembre == noMembreCo))) &&
                                    //Uniquement ActivitesQueJeParticipe
                                    ((rechercheActivite.uniquementActivitesQueJeParticipe == false ? true : (a.membresParticipants.Select(m => m.noMembre).Contains((int)noMembreCo)))) &&
                                    //Ne pas voir les activités dont le membre organisateur nous a dans sa liste noire
                                    (!a.membreOrganisateur.listeNoire.Select(m => m.noMembre).Contains(leMembreCo.noMembre))
                        );
                }
            }

            listActiviteTrouves = listActiviteTrouves
                        .Where(a => ((rechercheActivite.noTheme == null ? true : false) || a.noTheme == rechercheActivite.noTheme)
                        );

            //Ne doit pas changé de place, car c'est fait avec les critères
            IQueryable<Activite> listActiviteTrouvesPourDates = listActiviteTrouves
                .Where(a => (DbFunctions.TruncateTime(DateTime.Now) <= DbFunctions.TruncateTime(a.date))
                );

            listActiviteTrouves = listActiviteTrouves
                .Where(a => ((rechercheActivite.dateRecherche == null ? true : false) || DbFunctions.TruncateTime(a.date) == DbFunctions.TruncateTime(rechercheActivite.dateRecherche)) &&
                            (rechercheActivite.activiteFuturs == true ? a.date >= DateTime.Now : a.date < DateTime.Now)
                        );

            IQueryable<Activite> listActiviteTrouvesPreliminaire = listActiviteTrouves
                .Where(a => ((rechercheActivite.motCle == null ? true : false) || a.nom.ToLower().Contains(rechercheActivite.motCle.ToLower()))
                        );

            if (listActiviteTrouvesPreliminaire.Count() == 0)
            {
                listActiviteTrouves = listActiviteTrouves
                .Where(a => ((rechercheActivite.motCle == null ? true : false) || (a.nom.ToLower().Contains(rechercheActivite.motCle.ToLower()) || a.description.ToLower().Contains(rechercheActivite.motCle.ToLower())))
                        );

                listActiviteTrouvesPourDates = listActiviteTrouvesPourDates
                .Where(a => ((rechercheActivite.motCle == null ? true : false) || (a.nom.ToLower().Contains(rechercheActivite.motCle.ToLower()) || a.description.ToLower().Contains(rechercheActivite.motCle.ToLower())))
                );
            }
            else
            {
                listActiviteTrouves = listActiviteTrouvesPreliminaire;

                listActiviteTrouvesPourDates = listActiviteTrouvesPourDates
                .Where(a => ((rechercheActivite.motCle == null ? true : false) || a.nom.ToLower().Contains(rechercheActivite.motCle.ToLower()))
                );
            }

            rechercheActivite.datesOuIlYAActivites = listActiviteTrouvesPourDates.Select(m => DbFunctions.TruncateTime(m.date).Value).Distinct().ToList();

            rechercheActivite.nbActivitesTrouvesTotal = listActiviteTrouves.Count();

            if (rechercheActivite.activiteFuturs == true)
            {
                listActiviteTrouves = listActiviteTrouves.OrderBy(m => m.date).ThenBy(a => a.cout).ThenBy(a => a.noActivite);
            }
            else
            {
                listActiviteTrouves = listActiviteTrouves.OrderByDescending(m => m.date).ThenBy(a => a.cout).ThenBy(a => a.noActivite);
            }

            return listActiviteTrouves;
        }
    }
}