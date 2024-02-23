using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace CitiesManager.Core.DTO.Validators
{
    public class PersonDTOValidator : AbstractValidator <PersonDTO>
    {
        public PersonDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .Length(0, 10);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Age)
                .NotEmpty()
                .InclusiveBetween(18, 60);

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Length(10)
                .Must(phoneNumber => phoneNumber.All(char.IsDigit))
                .WithMessage("Enter a valid 10 digit phone number.");
        }
    }
}
