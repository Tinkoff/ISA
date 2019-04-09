using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tinkoff.ISA.Domain.Application;

namespace Tinkoff.ISA.DAL.Storage.Dao.Application
{
    public interface IApplicationPropertyDao
    {
        Task UpsertPropertyAsync<TProperty>(Expression<Func<ApplicationProperty, TProperty>> updateExpr, TProperty value);

        Task<ApplicationProperty> GetAsync();
    }
}
