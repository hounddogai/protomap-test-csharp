namespace Acme.Workforce.Contractors.Engagement.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement policy is in its life cycle.</summary>
public enum EngagementPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A engagement policy as the engagement team keeps it.</summary>
public sealed class EngagementPolicy(string code, string title, double floorValue, int sequence, int updatedAt, string team, bool active, bool approved, double minimum, double score, EngagementPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Sequence { get; set; } = sequence;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Team { get; set; } = team;
    public bool Active { get; set; } = active;
    public bool Approved { get; set; } = approved;
    public double Minimum { get; set; } = minimum;
    public double Score { get; set; } = score;
    public EngagementPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a engagement policy in status planned.</summary>
    public static EngagementPolicy NewEngagementPolicy(string code, string title)
    {
        return new EngagementPolicy(
            code,
            title,
            0.0,
            0,
            0,
            "",
            false,
            false,
            0.0,
            0.0,
            EngagementPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementPolicyStatus next)
    {
        switch (Status)
        {
            case EngagementPolicyStatus.Planned:
                return next == EngagementPolicyStatus.InProgress;
            case EngagementPolicyStatus.Scheduled:
                return next == EngagementPolicyStatus.InProgress;
            case EngagementPolicyStatus.InProgress:
                switch (next)
                {
                    case EngagementPolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case EngagementPolicyStatus.Completed:
                return next == EngagementPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(EngagementPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"engagement policy"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(FloorValue - Minimum, 2);
    }

    public EngagementPolicy Copy()
    {
        return new EngagementPolicy(
            Code,
            Title,
            FloorValue,
            Sequence,
            UpdatedAt,
            Team,
            Active,
            Approved,
            Minimum,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
