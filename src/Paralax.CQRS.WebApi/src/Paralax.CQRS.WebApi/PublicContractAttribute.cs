using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.CQRS.WebApi
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PublicContractAttribute : Attribute
    {
    }
}