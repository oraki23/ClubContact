/*------------------------------------------------------------------------------------

CLASSE UTILITAIRE PERMETTANT LES DIFFÉRENTS CALCULS POUR LES STATISTIQUES DE LA PARTIE ADMINISTRATEUR

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using ProjetSiteDeRencontre.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ProjetSiteDeRencontre.LesUtilitaires
{
    public static class Statistiques
    {
        public static IQueryable<Membre> ListeMembres(bool? premium = null, int? ageMin = null, int? ageMax = null, int? noProvince = null, int? noVille = null, bool membresDesactiver = false)
        {
            ClubContactContext db = new ClubContactContext();

            #region trouverAgeEnChiffre
            //DateTime? dateAgeMax = null;
            //DateTime? dateAgeMin = null;

            //if (ageMax != null)
            //{
            //    dateAgeMax = DateTime.Now.AddYears(-(int)ageMax);
            //}

            //if (ageMin != null)
            //{
            //    dateAgeMin = DateTime.Now.AddYears(-(int)ageMin);
            //}
            #endregion

            IQueryable<Membre> lesMembres = db.Membres;

            if (ageMin != null)
            {
                lesMembres = (from m in lesMembres
                                let years = DateTime.Now.Year - ((DateTime)m.dateNaissance).Year
                                  let birthdayThisYear = DbFunctions.AddYears(m.dateNaissance, years)

                                  where ageMax >= (birthdayThisYear > DateTime.Now ? years - 1 : years)
                                  select m
                              );
            }
            if (ageMax != null)
            {
                lesMembres = (from m in lesMembres
                                let years = DateTime.Now.Year - ((DateTime)m.dateNaissance).Year
                                  let birthdayThisYear = DbFunctions.AddYears(m.dateNaissance, years)

                                  where ageMin <= (birthdayThisYear > DateTime.Now ? years - 1 : years)
                                  select m
                              );
            }

            return lesMembres
                .Where(m => (premium == null ? true : false || m.premium == premium) &&
                            (noProvince == null ? true : false || m.noProvince == noProvince) &&
                            (noVille == null ? true : false || m.noVille == noVille) &&

                            (membresDesactiver == false ? (m.compteSupprimeParAdmin == null && m.dateSuppressionDuCompte == null) :
                                                            (m.compteSupprimeParAdmin != null && m.dateSuppressionDuCompte != null))

                            //((dateAgeMax == null ? true : false) || m.dateNaissance >= dateAgeMax) &&
                            //((dateAgeMin == null ? true : false) || m.dateNaissance <= dateAgeMin)
                            );
        }

        public static IQueryable<Signalement> ListeSignalement(int? noEtatSignalement = null)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Signalements
                    .Where(m => (noEtatSignalement == null ? true : false || m.noEtatSignalementActuel == noEtatSignalement)
                );
        }

        public static int NbAbonnement(int? forfait, int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Abonnements.Where(m => (forfait == null ? true : false || m.typeAbonnement == forfait) &&

                                             //Gestion des dates
                                             (anneeDebut == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year > anneeDebut) ||
                                                    (m.datePaiement.Year == anneeDebut && (moisDebut == null || m.datePaiement.Month > moisDebut)) ||
                                                    (m.datePaiement.Year == anneeDebut && m.datePaiement.Month == moisDebut && (jourDebut == null || m.datePaiement.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year < anneeFin) ||
                                                    (m.datePaiement.Year == anneeFin && (moisFin == null || m.datePaiement.Month < moisFin)) ||
                                                    (m.datePaiement.Year == anneeFin && m.datePaiement.Month == moisFin && (jourFin == null || m.datePaiement.Day <= jourFin))
                                                )
                                            )
                                 ).Count();
        }

        public static IQueryable<Abonnement> LesAbonnements(int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Abonnements.Where(m => 
                                            (anneeDebut == null ? true : false || 
                                                (
                                                    (m.datePaiement.Year > anneeDebut) ||
                                                    (m.datePaiement.Year == anneeDebut && (moisDebut == null || m.datePaiement.Month > moisDebut)) ||
                                                    (m.datePaiement.Year == anneeDebut && m.datePaiement.Month == moisDebut && (jourDebut == null || m.datePaiement.Day >= jourDebut))
                                                )
                                            
                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year < anneeFin) ||
                                                    (m.datePaiement.Year == anneeFin && (moisFin == null || m.datePaiement.Month < moisFin)) ||
                                                    (m.datePaiement.Year == anneeFin && m.datePaiement.Month == moisFin && (jourFin == null || m.datePaiement.Day <= jourFin))
                                                )
                                            )
                                 );
        }

        public static int NbDesabonnement(int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Abonnements.Where(m => (m.renouveler == false) &&
                                             (anneeDebut == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year > anneeDebut) ||
                                                    (m.datePaiement.Year == anneeDebut && (moisDebut == null || m.datePaiement.Month > moisDebut)) ||
                                                    (m.datePaiement.Year == anneeDebut && m.datePaiement.Month == moisDebut && (jourDebut == null || m.datePaiement.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year < anneeFin) ||
                                                    (m.datePaiement.Year == anneeFin && (moisFin == null || m.datePaiement.Month < moisFin)) ||
                                                    (m.datePaiement.Year == anneeFin && m.datePaiement.Month == moisFin && (jourFin == null || m.datePaiement.Day <= jourFin))
                                                )
                                            )
                                 ).Count();
        }

        public static double Revenu(int? forfait, int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Abonnements.Where(m => (forfait == null ? true : false || m.typeAbonnement == forfait) &&
                                             (anneeDebut == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year > anneeDebut) ||
                                                    (m.datePaiement.Year == anneeDebut && (moisDebut == null || m.datePaiement.Month > moisDebut)) ||
                                                    (m.datePaiement.Year == anneeDebut && m.datePaiement.Month == moisDebut && (jourDebut == null || m.datePaiement.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year < anneeFin) ||
                                                    (m.datePaiement.Year == anneeFin && (moisFin == null || m.datePaiement.Month < moisFin)) ||
                                                    (m.datePaiement.Year == anneeFin && m.datePaiement.Month == moisFin && (jourFin == null || m.datePaiement.Day <= jourFin))
                                                )
                                            )
                                 ).Sum(m => (double?) m.coutTotal) ?? 0.00;
        }

        public static double RevenuTPS(int? forfait, int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Abonnements.Where(m => (forfait == null ? true : false || m.typeAbonnement == forfait) &&
                                             (anneeDebut == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year > anneeDebut) ||
                                                    (m.datePaiement.Year == anneeDebut && (moisDebut == null || m.datePaiement.Month > moisDebut)) ||
                                                    (m.datePaiement.Year == anneeDebut && m.datePaiement.Month == moisDebut && (jourDebut == null || m.datePaiement.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year < anneeFin) ||
                                                    (m.datePaiement.Year == anneeFin && (moisFin == null || m.datePaiement.Month < moisFin)) ||
                                                    (m.datePaiement.Year == anneeFin && m.datePaiement.Month == moisFin && (jourFin == null || m.datePaiement.Day <= jourFin))
                                                )
                                            )
                                 ).Sum(m => (double?)m.prixTPS) ?? 0.00;
        }

        public static double RevenuTVQ(int? forfait, int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Abonnements.Where(m => (forfait == null ? true : false || m.typeAbonnement == forfait) &&
                                             (anneeDebut == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year > anneeDebut) ||
                                                    (m.datePaiement.Year == anneeDebut && (moisDebut == null || m.datePaiement.Month > moisDebut)) ||
                                                    (m.datePaiement.Year == anneeDebut && m.datePaiement.Month == moisDebut && (jourDebut == null || m.datePaiement.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.datePaiement.Year < anneeFin) ||
                                                    (m.datePaiement.Year == anneeFin && (moisFin == null || m.datePaiement.Month < moisFin)) ||
                                                    (m.datePaiement.Year == anneeFin && m.datePaiement.Month == moisFin && (jourFin == null || m.datePaiement.Day <= jourFin))
                                                )
                                            )
                                 ).Sum(m => (double?)m.prixTVQ) ?? 0.00;
        }

        //Signalement
        public static int NbSignalementsRecu(bool? traiteUniquement = null, bool? nouveauUniquement = null)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Signalements.Where(m => ((traiteUniquement == null ? true : false) || (traiteUniquement == true ? m.etatSignalementActuel.nomEtatSignalement == "Traité" : true)) &&
                                              ((nouveauUniquement == null ? true : false) || (nouveauUniquement == true ? m.etatSignalementActuel.nomEtatSignalement == "Nouveau" : true))
                                ).Count();
        }

        public static int NbMembresSupprimeAdmin()
        {
            ClubContactContext db = new ClubContactContext();

            return db.Membres.Where(m => m.compteSupprimeParAdmin == true).Count();
        }

        //Visites
        public static int NbConnexion(int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin, bool? premium)
        {
            ClubContactContext db = new ClubContactContext();

            return db.Connexions.Where(m => (m.premiumAuMomentDeConnexion == premium) &&
                                             (anneeDebut == null ? true : false ||
                                                (
                                                    (m.dateConnexion.Year > anneeDebut) ||
                                                    (m.dateConnexion.Year == anneeDebut && (moisDebut == null || m.dateConnexion.Month > moisDebut)) ||
                                                    (m.dateConnexion.Year == anneeDebut && m.dateConnexion.Month == moisDebut && (jourDebut == null || m.dateConnexion.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.dateConnexion.Year < anneeFin) ||
                                                    (m.dateConnexion.Year == anneeFin && (moisFin == null || m.dateConnexion.Month < moisFin)) ||
                                                    (m.dateConnexion.Year == anneeFin && m.dateConnexion.Month == moisFin && (jourFin == null || m.dateConnexion.Day <= jourFin))
                                                )
                                            )
                                 ).Count();
        }

        public static int NbMessagesOuCadeaux(int? anneeDebut, int? moisDebut, int? jourDebut, int? anneeFin, int? moisFin, int? jourFin, bool cadeaux = false)
        {
            ClubContactContext db = new ClubContactContext();

            if(cadeaux == true)
            {
                return db.Gifts.Where(m => (anneeDebut == null ? true : false ||
                                                (
                                                    (m.dateEnvoi.Year > anneeDebut) ||
                                                    (m.dateEnvoi.Year == anneeDebut && (moisDebut == null || m.dateEnvoi.Month > moisDebut)) ||
                                                    (m.dateEnvoi.Year == anneeDebut && m.dateEnvoi.Month == moisDebut && (jourDebut == null || m.dateEnvoi.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.dateEnvoi.Year < anneeFin) ||
                                                    (m.dateEnvoi.Year == anneeFin && (moisFin == null || m.dateEnvoi.Month < moisFin)) ||
                                                    (m.dateEnvoi.Year == anneeFin && m.dateEnvoi.Month == moisFin && (jourFin == null || m.dateEnvoi.Day <= jourFin))
                                                )
                                            )
                                 ).Count();
            }
            else
            {
                return db.Messages.Where(m =>
                                            (anneeDebut == null ? true : false ||
                                                (
                                                    (m.dateEnvoi.Year > anneeDebut) ||
                                                    (m.dateEnvoi.Year == anneeDebut && (moisDebut == null || m.dateEnvoi.Month > moisDebut)) ||
                                                    (m.dateEnvoi.Year == anneeDebut && m.dateEnvoi.Month == moisDebut && (jourDebut == null || m.dateEnvoi.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.dateEnvoi.Year < anneeFin) ||
                                                    (m.dateEnvoi.Year == anneeFin && (moisFin == null || m.dateEnvoi.Month < moisFin)) ||
                                                    (m.dateEnvoi.Year == anneeFin && m.dateEnvoi.Month == moisFin && (jourFin == null || m.dateEnvoi.Day <= jourFin))
                                                )
                                            ) &&
                                            (m.membreEnvoyeur != null)
                                 ).Count();
            }

            
        }

        public static int NbActiviteEtParticipantsOrganises(bool participantACalculer, bool? annulee, bool? payant, int? noTheme, int? noProvince, int? noVille,
                                            int? anneeDebut, int? moisDebut, int? jourDebut,
                                            int? anneeFin, int? moisFin, int? jourFin)
        {
            ClubContactContext db = new ClubContactContext();

            IQueryable<Activite> activitesQuiCorrespondent = db.Activites.Where(m => (annulee == null ? true : false || m.annulee == annulee) &&
                                            (payant == null ? true : false || (payant == true ? m.cout > 0 : (m.cout == 0 || m.cout == null))) &&
                                            (noTheme == null ? true : false || m.noTheme == noTheme) &&

                                            (noProvince == null ? true : false || m.noProvince == noProvince) &&
                                            (noVille == null ? true : false || m.noVille == noVille) &&

                                            (anneeDebut == null ? true : false ||
                                                (
                                                    (m.date.Year > anneeDebut) ||
                                                    (m.date.Year == anneeDebut && (moisDebut == null || m.date.Month > moisDebut)) ||
                                                    (m.date.Year == anneeDebut && m.date.Month == moisDebut && (jourDebut == null || m.date.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.date.Year < anneeFin) ||
                                                    (m.date.Year == anneeFin && (moisFin == null || m.date.Month < moisFin)) ||
                                                    (m.date.Year == anneeFin && m.date.Month == moisFin && (jourFin == null || m.date.Day <= jourFin))
                                                )
                                            )
                                 );

            //Nb d'activités
            if (participantACalculer == false)
            {
                return activitesQuiCorrespondent.Count();
            }
            //NbParticipants
            else
            {
                return activitesQuiCorrespondent.Sum(m => (int?)m.membresParticipants.Count()) ?? 0;
            }
        }

        public static IQueryable<Membre> ListeOrganisateursActivite(bool? annulee, bool? payant, int? noTheme, int? noProvince, int? noVille,
                                            int? anneeDebut, int? moisDebut, int? jourDebut,
                                            int? anneeFin, int? moisFin, int? jourFin, bool organisateursDesactiver = false)
        {
            ClubContactContext db = new ClubContactContext();

            IQueryable<int> listeActiviteResultat = db.Activites.Where(m => (annulee == null ? true : false || m.annulee == annulee) &&
                                            (payant == null ? true : false || (payant == true ? m.cout > 0 : (m.cout == 0 || m.cout == null))) &&
                                            (noTheme == null ? true : false || m.noTheme == noTheme) &&

                                            (organisateursDesactiver == false ? (m.membreOrganisateur.compteSupprimeParAdmin == null && m.membreOrganisateur.dateSuppressionDuCompte == null) : true) &&

                                            (noProvince == null ? true : false || m.noProvince == noProvince) &&
                                            (noVille == null ? true : false || m.noVille == noVille) &&

                                            (anneeDebut == null ? true : false ||
                                                (
                                                    (m.date.Year > anneeDebut) ||
                                                    (m.date.Year == anneeDebut && (moisDebut == null || m.date.Month > moisDebut)) ||
                                                    (m.date.Year == anneeDebut && m.date.Month == moisDebut && (jourDebut == null || m.date.Day >= jourDebut))
                                                )

                                            ) &&
                                            (anneeFin == null ? true : false ||
                                                (
                                                    (m.date.Year < anneeFin) ||
                                                    (m.date.Year == anneeFin && (moisFin == null || m.date.Month < moisFin)) ||
                                                    (m.date.Year == anneeFin && m.date.Month == moisFin && (jourFin == null || m.date.Day <= jourFin))
                                                )
                                            )
                                 ).Select(m => m.noMembreOrganisateur);

            return db.Membres.Where(m => listeActiviteResultat.Contains(m.noMembre));
        }
    }
}