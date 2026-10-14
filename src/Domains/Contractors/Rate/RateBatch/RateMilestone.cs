namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate milestone is in its life cycle.</summary>
public enum RateMilestoneStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rate milestone as the rate team keeps it.</summary>
public sealed class RateMilestone(string code, string name, string department, string timezone, double ceiling, List<string> tags, string skill, int version, RateMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Timezone { get; set; } = timezone;
    public double Ceiling { get; set; } = ceiling;
    public List<string> Tags { get; set; } = tags;
    public string Skill { get; set; } = skill;
    public int Version { get; set; } = version;
    public RateMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a rate milestone in status planned.</summary>
    public static RateMilestone NewRateMilestone(string code, string name)
    {
        return new RateMilestone(code, name, "", "", 0.0, new List<string>(), "", 0, RateMilestoneStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateMilestoneStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateMilestoneStatus next)
    {
        switch (Status)
        {
            case RateMilestoneStatus.Planned:
                switch (next)
                {
                    case RateMilestoneStatus.InProgress:
                    case RateMilestoneStatus.Completed:
                        return true;
                }
                return false;
            case RateMilestoneStatus.Scheduled:
                return next == RateMilestoneStatus.InProgress || next == RateMilestoneStatus.Cancelled;
            case RateMilestoneStatus.InProgress:
                return next == RateMilestoneStatus.Completed || next == RateMilestoneStatus.Cancelled;
            case RateMilestoneStatus.Completed:
                return next == RateMilestoneStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RateMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Version - Ceiling, 2);
    }

    public RateMilestone Copy()
    {
        return new RateMilestone(Code, Name, Department, Timezone, Ceiling, Tags, Skill, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
