namespace Acme.Workforce.Engagement.Feedback.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal feedback milestone is in its life cycle.</summary>
public enum SeasonalFeedbackMilestoneStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A seasonal feedback milestone as the feedback team keeps it.</summary>
public sealed class SeasonalFeedbackMilestone(string code, string name, string summary, string managerId, int leaveDays, int expiresAt, int updatedAt, SeasonalFeedbackMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string ManagerId { get; set; } = managerId;
    public int LeaveDays { get; set; } = leaveDays;
    public int ExpiresAt { get; set; } = expiresAt;
    public int UpdatedAt { get; set; } = updatedAt;
    public SeasonalFeedbackMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal feedback milestone in status idle.</summary>
    public static SeasonalFeedbackMilestone NewSeasonalFeedbackMilestone(string code, string name)
    {
        return new SeasonalFeedbackMilestone(code, name, "", "", 0, 0, 0, SeasonalFeedbackMilestoneStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalFeedbackMilestoneStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalFeedbackMilestoneStatus next)
    {
        switch (Status)
        {
            case SeasonalFeedbackMilestoneStatus.Idle:
                switch (next)
                {
                    case SeasonalFeedbackMilestoneStatus.Warming:
                    case SeasonalFeedbackMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalFeedbackMilestoneStatus.Warming:
                switch (next)
                {
                    case SeasonalFeedbackMilestoneStatus.Draining:
                        return true;
                }
                return false;
            case SeasonalFeedbackMilestoneStatus.Ready:
                switch (next)
                {
                    case SeasonalFeedbackMilestoneStatus.Draining:
                    case SeasonalFeedbackMilestoneStatus.Stopped:
                        return true;
                }
                return false;
            case SeasonalFeedbackMilestoneStatus.Draining:
                return next == SeasonalFeedbackMilestoneStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SeasonalFeedbackMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal feedback milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal feedback milestone"} {Code}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)UpdatedAt + (double)LeaveDays, 2);
    }

    public SeasonalFeedbackMilestone Copy()
    {
        return new SeasonalFeedbackMilestone(
            Code,
            Name,
            Summary,
            ManagerId,
            LeaveDays,
            ExpiresAt,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
