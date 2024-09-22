using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Paralax.WebApi.Requests
{
    public interface IRequestHandler<in TRequest, TResult> where TRequest : class, IRequest 
    {
        Task<TResult> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}