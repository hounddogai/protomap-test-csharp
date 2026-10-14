namespace Acme.Workforce.Payrollops.PayRun.RunAllocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pay run cycle is in its life cycle.</summary>
public enum FallbackPayRunCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback pay run cycle. Its code is unique within a tenant.</summary>
public sealed class FallbackPayRunCycle(string code, string title, double hours, int attempts, string managerId, int createdAt, double margin, string currency, int limit, bool approved, string source, FallbackPayRunCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public int Attempts { get; set; } = attempts;
    public string ManagerId { get; set; } = managerId;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public string Currency { get; set; } = currency;
    public int Limit { get; set; } = limit;
    public bool Approved { get; set; } = approved;
    public string Source { get; set; } = source;
    public FallbackPayRunCycleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pay run cycle in status planned.</summary>
    public static FallbackPayRunCycle NewFallbackPayRunCycle(string code, string title)
    {
        return new FallbackPayRunCycle(
            code,
            title,
            0.0,
            0,
            "",
            0,
            0.0,
            "",
            0,
            false,
            "",
            FallbackPayRunCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPayRunCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPayRunCycleStatus next)
    {
        switch (Status)
        {
            case FallbackPayRunCycleStatus.Planned:
                switch (next)
                {
                    case FallbackPayRunCycleStatus.InProgress:
                    case FallbackPayRunCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPayRunCycleStatus.Scheduled:
                switch (next)
                {
                    case FallbackPayRunCycleStatus.Completed:
                    case FallbackPayRunCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPayRunCycleStatus.InProgress:
                switch (next)
                {
                    case FallbackPayRunCycleStatus.Completed:
                    case FallbackPayRunCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPayRunCycleStatus.Completed:
                switch (next)
                {
                    case FallbackPayRunCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPayRunCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pay run cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Attempts + (double)CreatedAt, 2);
    }

    public FallbackPayRunCycle Copy()
    {
        return new FallbackPayRunCycle(
            Code,
            Title,
            Hours,
            Attempts,
            ManagerId,
            CreatedAt,
            Margin,
            Currency,
            Limit,
            Approved,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
