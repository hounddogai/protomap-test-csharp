namespace Acme.Workforce.Talent.Nomination.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination milestone is in its life cycle.</summary>
public enum NominationMilestoneStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One nomination milestone, with what the planners need to act on it.</summary>
public sealed class NominationMilestone(string code, string title, int revision, int capacity, List<string> tags, string jobCode, int graceDays, string phone, double minimum, bool active, NominationMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int Capacity { get; set; } = capacity;
    public List<string> Tags { get; set; } = tags;
    public string JobCode { get; set; } = jobCode;
    public int GraceDays { get; set; } = graceDays;
    public string Phone { get; set; } = phone;
    public double Minimum { get; set; } = minimum;
    public bool Active { get; set; } = active;
    public NominationMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a nomination milestone in status created.</summary>
    public static NominationMilestone NewNominationMilestone(string code, string title)
    {
        return new NominationMilestone(
            code,
            title,
            0,
            0,
            new List<string>(),
            "",
            0,
            "",
            0.0,
            false,
            NominationMilestoneStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationMilestoneStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationMilestoneStatus next)
    {
        switch (Status)
        {
            case NominationMilestoneStatus.Created:
                return next == NominationMilestoneStatus.Retired;
            case NominationMilestoneStatus.Validated:
                switch (next)
                {
                    case NominationMilestoneStatus.Published:
                    case NominationMilestoneStatus.Retired:
                        return true;
                }
                return false;
            case NominationMilestoneStatus.Published:
                switch (next)
                {
                    case NominationMilestoneStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double WeightedExposure()
    {
        return Math.Round(Minimum * (double)GraceDays, 2);
    }

    public NominationMilestone Copy()
    {
        return new NominationMilestone(
            Code,
            Title,
            Revision,
            Capacity,
            Tags,
            JobCode,
            GraceDays,
            Phone,
            Minimum,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
