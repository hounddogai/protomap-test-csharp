namespace Acme.Workforce.Talent.Pool.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pool estimate is in its life cycle.</summary>
public enum PoolEstimateStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A pool estimate as the pool team keeps it.</summary>
public sealed class PoolEstimate(string code, string title, int limit, string jobCode, string region, PoolEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public string Region { get; set; } = region;
    public PoolEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a pool estimate in status proposed.</summary>
    public static PoolEstimate NewPoolEstimate(string code, string title)
    {
        return new PoolEstimate(code, title, 0, "", "", PoolEstimateStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PoolEstimateStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PoolEstimateStatus next)
    {
        switch (Status)
        {
            case PoolEstimateStatus.Proposed:
                switch (next)
                {
                    case PoolEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolEstimateStatus.Approved:
                switch (next)
                {
                    case PoolEstimateStatus.Rejected:
                    case PoolEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
            case PoolEstimateStatus.Rejected:
                switch (next)
                {
                    case PoolEstimateStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PoolEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pool estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public PoolEstimate Copy()
    {
        return new PoolEstimate(Code, Title, Limit, JobCode, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
