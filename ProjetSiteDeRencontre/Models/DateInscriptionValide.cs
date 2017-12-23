/*------------------------------------------------------------------------------------

CLASSE CONTENANT DES VALIDATION D'ATTRIBUTES CUSTOM:
    DATEINSCRIPTIONVALIDE: VÉRIFIE SI LA DATE D'INSCRIPTION AFIN DE S'ASSURER QUE LE MEMBRE À 18 ANS.

--------------------------------------------------------------------------------------
Par: Anthony Brochu et Marie-Ève Massé
Novembre 2017
Club Contact
------------------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProjetSiteDeRencontre.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DateInscriptionValide : ValidationAttribute, IClientValidatable
    {
        private readonly DateTime _maxValue = DateTime.UtcNow.AddYears(-18);

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            try
            {
                if ((DateTime)value <= _maxValue)
                {
                    return ValidationResult.Success;
                }
                else
                {
                    return new ValidationResult("Vous devez avoir 18 ans et plus pour vous inscrire.");
                }
            }
            catch (Exception ex)
            {
                // Do stuff, i.e. log the exception
                // Let it go through the upper levels, something bad happened
                throw ex;
            }
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            //string errorMessage = this.FormatErrorMessage(metadata.DisplayName);
            string errorMessage = ErrorMessageString;

            // The value we set here are needed by the jQuery adapter
            ModelClientValidationRule datePlusPetiteQue = new ModelClientValidationRule();
            datePlusPetiteQue.ErrorMessage = errorMessage;
            datePlusPetiteQue.ValidationType = "datepluspetitque"; // This is the name the jQuery adapter will use
            //"otherpropertyname" is the name of the jQuery parameter for the adapter, must be LOWERCASE!
            datePlusPetiteQue.ValidationParameters.Add("maxvalue", _maxValue.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);

            yield return datePlusPetiteQue;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    //public class DateActivitePlusGrandQuAujourdhui : ValidationAttribute, IClientValidatable
    //{
    //    private readonly DateTime _minValue = DateTime.Now;

    //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    //    {
    //        try
    //        {
    //            if ((DateTime)value >= _minValue)
    //            {
    //                return ValidationResult.Success;
    //            }
    //            else
    //            {
    //                return new ValidationResult("Veuillez choisir une date dans le futur.");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            // Do stuff, i.e. log the exception
    //            // Let it go through the upper levels, something bad happened
    //            throw ex;
    //        }
    //    }

    //    public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
    //    {
    //        //string errorMessage = this.FormatErrorMessage(metadata.DisplayName);
    //        string errorMessage = ErrorMessageString;

    //        // The value we set here are needed by the jQuery adapter
    //        ModelClientValidationRule datePlusGrandQue = new ModelClientValidationRule();
    //        datePlusGrandQue.ErrorMessage = errorMessage;
    //        datePlusGrandQue.ValidationType = "dateplusgrandque"; // This is the name the jQuery adapter will use
    //        //"otherpropertyname" is the name of the jQuery parameter for the adapter, must be LOWERCASE!
    //        datePlusGrandQue.ValidationParameters.Add("minvalue", _minValue.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);

    //        yield return datePlusGrandQue;
    //    }
    //}
}