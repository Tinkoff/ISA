using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Tinkoff.ISA.DAL.Storage.Common
{
    public interface IRepository<TEntity, in TKey>
        where TEntity : class
    {
        Task<ICollection<TEntity>> Find(Expression<Func<TEntity, bool>> filterExpr);

        Task<TEntity> FirstOrDefault(Expression<Func<TEntity, bool>> filterExpr);

        Task<TEntity> Get(TKey id);

        Task Add(TEntity entity);

        Task<long> Count();
    }
}
