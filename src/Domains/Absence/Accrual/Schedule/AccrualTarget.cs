namespace Acme.Workforce.Absence.Accrual.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual target is in its life cycle.</summary>
public enum AccrualTargetStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A accrual target as the accrual team keeps it.</summary>
public sealed class AccrualTarget(string code, string name, string region, string channel, int quantity, double threshold, string source, int createdAt, string managerId, int retries, int graceDays, AccrualTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string Channel { get; set; } = channel;
    public int Quantity { get; set; } = quantity;
    public double Threshold { get; set; } = threshold;
    public string Source { get; set; } = source;
    public int CreatedAt { get; set; } = createdAt;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public int GraceDays { get; set; } = graceDays;
    public AccrualTargetStatus Status { get; set; } = status;

    /// <summary>Starts a accrual target in status new.</summary>
    public static AccrualTarget NewAccrualTarget(string code, string name)
    {
        return new AccrualTarget(code, name, "", "", 0, 0.0, "", 0, "", 0, 0, AccrualTargetStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualTargetStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualTargetStatus next)
    {
        switch (Status)
        {
            case AccrualTargetStatus.New:
                return next == AccrualTargetStatus.Failed;
            case AccrualTargetStatus.Queued:
                return next == AccrualTargetStatus.Running || next == AccrualTargetStatus.Done;
            case AccrualTargetStatus.Running:
                switch (next)
                {
                    case AccrualTargetStatus.Done:
                    case AccrualTargetStatus.Failed:
                        return true;
                }
                return false;
            case AccrualTargetStatus.Done:
                switch (next)
                {
                    case AccrualTargetStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Retries + (double)GraceDays, 2);
    }

    public AccrualTarget Copy()
    {
        return new AccrualTarget(
            Code,
            Name,
            Region,
            Channel,
            Quantity,
            Threshold,
            Source,
            CreatedAt,
            ManagerId,
            Retries,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
