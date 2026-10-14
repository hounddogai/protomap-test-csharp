namespace Acme.Workforce.Compensation.Benchmark.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive benchmark milestone is in its life cycle.</summary>
public enum AdaptiveBenchmarkMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One adaptive benchmark milestone, with what the planners need to act on it.</summary>
public sealed class AdaptiveBenchmarkMilestone(string code, string name, double threshold, bool archived, int revision, string phone, double amount, double ceiling, int expiresAt, string currency, string managerId, AdaptiveBenchmarkMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public bool Archived { get; set; } = archived;
    public int Revision { get; set; } = revision;
    public string Phone { get; set; } = phone;
    public double Amount { get; set; } = amount;
    public double Ceiling { get; set; } = ceiling;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Currency { get; set; } = currency;
    public string ManagerId { get; set; } = managerId;
    public AdaptiveBenchmarkMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive benchmark milestone in status planned.</summary>
    public static AdaptiveBenchmarkMilestone NewAdaptiveBenchmarkMilestone(string code, string name)
    {
        return new AdaptiveBenchmarkMilestone(
            code,
            name,
            0.0,
            false,
            0,
            "",
            0.0,
            0.0,
            0,
            "",
            "",
            AdaptiveBenchmarkMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveBenchmarkMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveBenchmarkMilestoneStatus next)
    {
        switch (Status)
        {
            case AdaptiveBenchmarkMilestoneStatus.Planned:
                switch (next)
                {
                    case AdaptiveBenchmarkMilestoneStatus.InProgress:
                        return true;
                }
                return false;
            case AdaptiveBenchmarkMilestoneStatus.Scheduled:
                return next == AdaptiveBenchmarkMilestoneStatus.Cancelled;
            case AdaptiveBenchmarkMilestoneStatus.InProgress:
                return next == AdaptiveBenchmarkMilestoneStatus.Completed || next == AdaptiveBenchmarkMilestoneStatus.Cancelled;
            case AdaptiveBenchmarkMilestoneStatus.Completed:
                return next == AdaptiveBenchmarkMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(AdaptiveBenchmarkMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive benchmark milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Amount + (double)ExpiresAt, 2);
    }

    public AdaptiveBenchmarkMilestone Copy()
    {
        return new AdaptiveBenchmarkMilestone(
            Code,
            Name,
            Threshold,
            Archived,
            Revision,
            Phone,
            Amount,
            Ceiling,
            ExpiresAt,
            Currency,
            ManagerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
