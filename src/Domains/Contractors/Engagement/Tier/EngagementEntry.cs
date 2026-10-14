namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement entry is in its life cycle.</summary>
public enum EngagementEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A engagement entry as the engagement team keeps it.</summary>
public sealed class EngagementEntry(string code, string label, double floorValue, string team, List<string> tags, EngagementEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public List<string> Tags { get; set; } = tags;
    public EngagementEntryStatus Status { get; set; } = status;

    /// <summary>Starts a engagement entry in status draft.</summary>
    public static EngagementEntry NewEngagementEntry(string code, string label)
    {
        return new EngagementEntry(code, label, 0.0, "", new List<string>(), EngagementEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementEntryStatus next)
    {
        switch (Status)
        {
            case EngagementEntryStatus.Draft:
                return next == EngagementEntryStatus.Active || next == EngagementEntryStatus.Suspended;
            case EngagementEntryStatus.Pending:
                return next == EngagementEntryStatus.Closed;
            case EngagementEntryStatus.Active:
                return next == EngagementEntryStatus.Closed;
            case EngagementEntryStatus.Suspended:
                switch (next)
                {
                    case EngagementEntryStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EngagementEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public EngagementEntry Copy()
    {
        return new EngagementEntry(Code, Label, FloorValue, Team, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
