namespace Acme.Workforce.Workplace.Visitor.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending visitor adjustment is in its life cycle.</summary>
public enum PendingVisitorAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One pending visitor adjustment, with what the planners need to act on it.</summary>
public sealed class PendingVisitorAdjustment(string code, string title, string region, double margin, string description, int version, int createdAt, int leaveDays, PendingVisitorAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public double Margin { get; set; } = margin;
    public string Description { get; set; } = description;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public int LeaveDays { get; set; } = leaveDays;
    public PendingVisitorAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending visitor adjustment in status idle.</summary>
    public static PendingVisitorAdjustment NewPendingVisitorAdjustment(string code, string title)
    {
        return new PendingVisitorAdjustment(code, title, "", 0.0, "", 0, 0, 0, PendingVisitorAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingVisitorAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingVisitorAdjustmentStatus next)
    {
        switch (Status)
        {
            case PendingVisitorAdjustmentStatus.Idle:
                return next == PendingVisitorAdjustmentStatus.Stopped;
            case PendingVisitorAdjustmentStatus.Warming:
                return next == PendingVisitorAdjustmentStatus.Stopped;
            case PendingVisitorAdjustmentStatus.Ready:
                return next == PendingVisitorAdjustmentStatus.Stopped;
            case PendingVisitorAdjustmentStatus.Draining:
                switch (next)
                {
                    case PendingVisitorAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingVisitorAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending visitor adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"pending visitor adjustment"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Margin + (double)CreatedAt, 2);
    }

    public PendingVisitorAdjustment Copy()
    {
        return new PendingVisitorAdjustment(
            Code,
            Title,
            Region,
            Margin,
            Description,
            Version,
            CreatedAt,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
