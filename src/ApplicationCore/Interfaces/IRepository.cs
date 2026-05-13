using Ardalis.Specification;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

/// <summary>Write repository for aggregate roots; extends <see cref="IReadRepository{T}"/> with add/update/delete operations.</summary>
public interface IRepository<T> : IRepositoryBase<T> where T : class, IAggregateRoot
{
}
