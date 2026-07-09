using FraudDetection.Application.Interfaces.Repositories;
using FraudDetection.Domain.Entities;
using FraudDetection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Repositories;

internal sealed class FraudReportRepository : IFraudReportRepository
{
    private readonly FraudDetectionDbContext _context;

    public FraudReportRepository(FraudDetectionDbContext context) => _context = context;

    public async Task<FraudReport?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.FraudReports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<FraudReport> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? customerId = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FraudReports.AsNoTracking();

        if (customerId.HasValue)
            query = query.Where(r => r.CustomerId == customerId.Value);

        if (from.HasValue)
            query = query.Where(r => r.GeneratedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.GeneratedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(r => r.GeneratedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<FraudReport?> FindCachedAsync(
        Guid? customerId,
        DateTime windowFrom,
        DateTime windowTo,
        DateTime notBefore,
        CancellationToken cancellationToken = default)
        => await _context.FraudReports
            .AsNoTracking()
            .Where(r => r.CustomerId == customerId
                     && r.WindowFrom == windowFrom
                     && r.WindowTo == windowTo
                     && r.GeneratedAt >= notBefore)
            .OrderByDescending(r => r.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAsync(FraudReport report, CancellationToken cancellationToken = default)
        => await _context.FraudReports.AddAsync(report, cancellationToken);
}
