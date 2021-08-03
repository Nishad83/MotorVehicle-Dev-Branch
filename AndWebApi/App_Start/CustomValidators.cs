using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AndWebApi.App_Start
{
    public class CustomValidators
    {

        public class RequiredIfAttribute : ValidationAttribute, IClientValidatable
        {
            private String PropertyName { get; set; }
            private Object DesiredValue { get; set; }
            private readonly RequiredAttribute _innerAttribute;

            public RequiredIfAttribute(String propertyName, Object desiredvalue)
            {
                PropertyName = propertyName;
                DesiredValue = desiredvalue;
                _innerAttribute = new RequiredAttribute();
            }

            protected override ValidationResult IsValid(object value, ValidationContext context)
            {
                var dependentValue = context.ObjectInstance.GetType().GetProperty(PropertyName).GetValue(context.ObjectInstance, null);

                if (dependentValue.ToString() == DesiredValue.ToString())
                {
                    if (!_innerAttribute.IsValid(value))
                    {
                        return new ValidationResult(FormatErrorMessage(context.DisplayName), new[] { context.MemberName });
                    }
                }
                return ValidationResult.Success;
            }

            public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
            {
                var rule = new ModelClientValidationRule
                {
                    ErrorMessage = ErrorMessageString,
                    ValidationType = "requiredif",
                };
                rule.ValidationParameters["dependentproperty"] = (context as ViewContext).ViewData.TemplateInfo.GetFullHtmlFieldId(PropertyName);
                rule.ValidationParameters["desiredvalue"] = DesiredValue is bool ? DesiredValue.ToString().ToLower() : DesiredValue;

                yield return rule;
            }
        }

        //public class RequiredIfAttribute : ValidationAttribute
        //{
        //    private const string DefaultErrorMessageFormatString = "The {0} field is required.";
        //    private readonly string[] _dependentProperties;
        //    public string PropertyName { get; set; }
        //    public object Value { get; set; }

        //    public RequiredIfAttribute(string propertyName, object value, string errorMessage = "")
        //    {
        //        PropertyName = propertyName;
        //        ErrorMessage = errorMessage;
        //        Value = value;
        //    }

        //    //public RequiredIfAttribute(string[] dependentProperties)
        //    //{
        //    //    _dependentProperties = dependentProperties;
        //    //    ErrorMessage = DefaultErrorMessageFormatString;
        //    //}

        //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        //    {
        //        var instance = validationContext.ObjectInstance;
        //        var type = instance.GetType();
        //        var proprtyvalue = type.GetProperty(PropertyName).GetValue(instance, null);
        //        if (proprtyvalue.ToString() == Value.ToString() && value == null)
        //        {
        //            return new ValidationResult(ErrorMessage);
        //        }
        //        return ValidationResult.Success;
        //    }
        //}
    }
}  
