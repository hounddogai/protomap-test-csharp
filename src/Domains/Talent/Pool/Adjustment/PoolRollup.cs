namespace Acme.Workforce.Talent.Pool.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool rollup is in its life cycle.</summary>
public enum PoolRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pool rollup. Its code is unique within a tenant.</summary>
public sealed class PoolRollup(string code, string title, string country, string unit, int endsAt, int retries, string employeeId, int tenureMonths, string team, string costCenter, PoolRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public string EmployeeId { get; set; } = employeeId;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public string CostCenter { get; set; } = costCenter;
    public PoolRollupStatus Status { get; set; } = status;

    /// <summary>Starts a pool rollup in status new.</summary>
    public static PoolRollup NewPoolRollup(string code, string title)
    {
        return new PoolRollup(code, title, "", "", 0, 0, "", 0, "", "", PoolRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolRollupStatus next)
    {
        switch (Status)
        {
            case PoolRollupStatus.New:
                return next == PoolRollupStatus.Queued || next == PoolRollupStatus.Failed;
            case PoolRollupStatus.Queued:
                return next == PoolRollupStatus.Running || next == PoolRollupStatus.Failed;
            case PoolRollupStatus.Running:
                switch (next)
                {
                    case PoolRollupStatus.Done:
                    case PoolRollupStatus.Failed:
                        return true;
                }
                return false;
            case PoolRollupStatus.Done:
                switch (next)
                {
                    case PoolRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pool rollup"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)EndsAt + (double)TenureMonths, 2);
    }

    public PoolRollup Copy()
    {
        return new PoolRollup(
            Code,
            Title,
            Country,
            Unit,
            EndsAt,
            Retries,
            EmployeeId,
            TenureMonths,
            Team,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
