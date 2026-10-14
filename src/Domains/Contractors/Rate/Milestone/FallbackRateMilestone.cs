namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback rate milestone is in its life cycle.</summary>
public enum FallbackRateMilestoneStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One fallback rate milestone, with what the planners need to act on it.</summary>
public sealed class FallbackRateMilestone(string code, string name, Dictionary<string, string> attributes, int revision, double margin, FallbackRateMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public double Margin { get; set; } = margin;
    public FallbackRateMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a fallback rate milestone in status proposed.</summary>
    public static FallbackRateMilestone NewFallbackRateMilestone(string code, string name)
    {
        return new FallbackRateMilestone(
            code,
            name,
            new Dictionary<string, string>(),
            0,
            0.0,
            FallbackRateMilestoneStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackRateMilestoneStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackRateMilestoneStatus next)
    {
        switch (Status)
        {
            case FallbackRateMilestoneStatus.Proposed:
                return next == FallbackRateMilestoneStatus.Approved;
            case FallbackRateMilestoneStatus.Approved:
                switch (next)
                {
                    case FallbackRateMilestoneStatus.Rejected:
                    case FallbackRateMilestoneStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackRateMilestoneStatus.Rejected:
                return next == FallbackRateMilestoneStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackRateMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback rate milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{"fallback rate milestone"} {Code}";
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

    public double NetLoad()
    {
        return Math.Round((double)Revision * Margin, 2);
    }

    public FallbackRateMilestone Copy()
    {
        return new FallbackRateMilestone(Code, Name, Attributes, Revision, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
