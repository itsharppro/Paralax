using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.Types
{
    public interface IIdentifiable<out T>
    {
        T Id { get; }
    }
}