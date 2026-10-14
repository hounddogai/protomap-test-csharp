namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted nomination milestone is in its life cycle.</summary>
public enum WeightedNominationMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One weighted nomination milestone, with what the planners need to act on it.</summary>
public sealed class WeightedNominationMilestone(string code, string name, int graceDays, int createdAt, string category, int startsAt, Dictionary<string, string> attributes, WeightedNominationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int GraceDays { get; set; } = graceDays;
    public int CreatedAt { get; set; } = createdAt;
    public string Category { get; set; } = category;
    public int StartsAt { get; set; } = startsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public WeightedNominationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a weighted nomination milestone in status planned.</summary>
    public static WeightedNominationMilestone NewWeightedNominationMilestone(string code, string name)
    {
        return new WeightedNominationMilestone(
            code,
            name,
            0,
            0,
            "",
            0,
            new Dictionary<string, string>(),
            WeightedNominationMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedNominationMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedNominationMilestoneStatus next)
    {
        switch (Status)
        {
            case WeightedNominationMilestoneStatus.Planned:
                return next == WeightedNominationMilestoneStatus.InProgress;
            case WeightedNominationMilestoneStatus.Scheduled:
                return next == WeightedNominationMilestoneStatus.InProgress || next == WeightedNominationMilestoneStatus.Completed;
            case WeightedNominationMilestoneStatus.InProgress:
                return next == WeightedNominationMilestoneStatus.Completed;
            case WeightedNominationMilestoneStatus.Completed:
                return next == WeightedNominationMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WeightedNominationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted nomination milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"weighted nomination milestone"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)GraceDays * (double)StartsAt, 2);
    }

    public WeightedNominationMilestone Copy()
    {
        return new WeightedNominationMilestone(
            Code,
            Name,
            GraceDays,
            CreatedAt,
            Category,
            StartsAt,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
