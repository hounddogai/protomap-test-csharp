namespace Acme.Workforce.Talent.Mobility.MobilityEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback mobility entry is in its life cycle.</summary>
public enum FallbackMobilityEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One fallback mobility entry, with what the planners need to act on it.</summary>
public sealed class FallbackMobilityEntry(string code, string title, double floorValue, int version, string skill, bool archived, string reviewer, FallbackMobilityEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public int Version { get; set; } = version;
    public string Skill { get; set; } = skill;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public FallbackMobilityEntryStatus Status { get; set; } = status;

    /// <summary>Starts a fallback mobility entry in status planned.</summary>
    public static FallbackMobilityEntry NewFallbackMobilityEntry(string code, string title)
    {
        return new FallbackMobilityEntry(code, title, 0.0, 0, "", false, "", FallbackMobilityEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackMobilityEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackMobilityEntryStatus next)
    {
        switch (Status)
        {
            case FallbackMobilityEntryStatus.Planned:
                switch (next)
                {
                    case FallbackMobilityEntryStatus.Completed:
                        return true;
                }
                return false;
            case FallbackMobilityEntryStatus.Scheduled:
                return next == FallbackMobilityEntryStatus.Completed;
            case FallbackMobilityEntryStatus.InProgress:
                return next == FallbackMobilityEntryStatus.Completed || next == FallbackMobilityEntryStatus.Cancelled;
            case FallbackMobilityEntryStatus.Completed:
                return next == FallbackMobilityEntryStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FallbackMobilityEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback mobility entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(FloorValue - (double)Version, 2);
    }

    public FallbackMobilityEntry Copy()
    {
        return new FallbackMobilityEntry(Code, Title, FloorValue, Version, Skill, Archived, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
