using Myfunction.Models.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myfunction.Static
{
    public static class ProductValidator
    {
        public static Dictionary<string, string[]> ValidateRequest(CreateProductRequest request)
        {
            var context = new ValidationContext(request);
            var results = new List<ValidationResult>();
            var errors = new Dictionary<string, string[]>();

            if (!Validator.TryValidateObject(request, context, results, true))
            {
                foreach (var result in results)
                {
                    var propertyName = result.MemberNames.FirstOrDefault() ?? "General";
                    if (!errors.ContainsKey(propertyName))
                    {
                        errors[propertyName] = new[] { result.ErrorMessage ?? "Validation error" };
                    }
                    else
                    {
                        errors[propertyName] = errors[propertyName]
                            .Concat(new[] { result.ErrorMessage ?? "Validation error" })
                            .ToArray();
                    }
                }
            }

            return errors;
        }
    }
}
