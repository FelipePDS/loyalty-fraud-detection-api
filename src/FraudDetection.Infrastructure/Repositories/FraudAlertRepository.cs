using FraudDetection.Application.Interfaces.Repositories;
using FraudDetection.Domain.Entities;
using FraudDetection.Domain.Enums;
using FraudDetection.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FraudDetection.Infrastructure.Repositories;

internal sealed class FraudAlertRepository : IFraudAlertRepository
{
    private readonly FraudDetectionDbContext _context;

    public FraudAlertRepository(FraudDetectionDbContext context) => _context = context;

    public async Task<FraudAlert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.FraudAlerts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<(IReadOnlyList<FraudAlert> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        Guid? customerId = null,
        AlertSeverity? severity = null,
        AlertStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FraudAlerts.AsNoTracking();

        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId.Value);

        if (severity.HasValue)
            query = query.Where(a => a.Severity == severity.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (from.HasValue)
            query = query.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<FraudAlert>> GetOpenByCustomerIdAsync(
        Guid customerId,
        DateTime since,
        CancellationToken cancellationToken = default)
        => await _context.FraudAlerts
            .AsNoTracking()
            .Where(a => a.CustomerId == customerId
                     && a.Status == AlertStatus.Open
                     && a.CreatedAt >= since)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<(AlertSeverity Severity, AlertStatus Status), int>> GetDashboardCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await _context.FraudAlerts
            .AsNoTracking()
            .GroupBy(a => new { a.Severity, a.Status })
            .Select(g => new { g.Key.Severity, g.Key.Status, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return groups.ToDictionary(
            g => (g.Severity, g.Status),
            g => g.Count);
    }

    public async Task AddAsync(FraudAlert alert, CancellationToken cancellationToken = default)
        => await _context.FraudAlerts.AddAsync(alert, cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<FraudAlert> alerts,
        CancellationToken cancellationToken = default)
        => await _context.FraudAlerts.AddRangeAsync(alerts, cancellationToken);

    public void Update(FraudAlert alert)
        => _context.FraudAlerts.Update(alert);
}
