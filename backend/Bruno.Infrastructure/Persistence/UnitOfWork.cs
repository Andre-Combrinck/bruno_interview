using Bruno.Application.Common.Interfaces;

namespace Bruno.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly BrunoDbContext _db;

    public UnitOfWork(BrunoDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
