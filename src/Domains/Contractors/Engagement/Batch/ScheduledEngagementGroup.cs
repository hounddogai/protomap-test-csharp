namespace Acme.Workforce.Contractors.Engagement.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled engagement group is in its life cycle.</summary>
public enum ScheduledEngagementGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One scheduled engagement group, with what the planners need to act on it.</summary>
public sealed class ScheduledEngagementGroup(string code, string name, string locale, int sequence, double budget, double score, string category, int graceDays, string skill, bool pinned, ScheduledEngagementGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public int Sequence { get; set; } = sequence;
    public double Budget { get; set; } = budget;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public int GraceDays { get; set; } = graceDays;
    public string Skill { get; set; } = skill;
    public bool Pinned { get; set; } = pinned;
    public ScheduledEngagementGroupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled engagement group in status idle.</summary>
    public static ScheduledEngagementGroup NewScheduledEngagementGroup(string code, string name)
    {
        return new ScheduledEngagementGroup(
            code,
            name,
            "",
            0,
            0.0,
            0.0,
            "",
            0,
            "",
            false,
            ScheduledEngagementGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEngagementGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEngagementGroupStatus next)
    {
        switch (Status)
        {
            case ScheduledEngagementGroupStatus.Idle:
                return next == ScheduledEngagementGroupStatus.Draining;
            case ScheduledEngagementGroupStatus.Warming:
                switch (next)
                {
                    case ScheduledEngagementGroupStatus.Ready:
                    case ScheduledEngagementGroupStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledEngagementGroupStatus.Ready:
                switch (next)
                {
                    case ScheduledEngagementGroupStatus.Draining:
                        return true;
                }
                return false;
            case ScheduledEngagementGroupStatus.Draining:
                switch (next)
                {
                    case ScheduledEngagementGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledEngagementGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled engagement group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)GraceDays - Budget, 2);
    }

    public ScheduledEngagementGroup Copy()
    {
        return new ScheduledEngagementGroup(
            Code,
            Name,
            Locale,
            Sequence,
            Budget,
            Score,
            Category,
            GraceDays,
            Skill,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
