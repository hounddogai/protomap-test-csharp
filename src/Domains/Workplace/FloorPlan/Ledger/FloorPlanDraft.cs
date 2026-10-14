namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan draft is in its life cycle.</summary>
public enum FloorPlanDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One floor plan draft, with what the planners need to act on it.</summary>
public sealed class FloorPlanDraft(string code, string name, string summary, string checksum, string team, int windowDays, int version, bool pinned, string title, int createdAt, FloorPlanDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public string Checksum { get; set; } = checksum;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public bool Pinned { get; set; } = pinned;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public FloorPlanDraftStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan draft in status planned.</summary>
    public static FloorPlanDraft NewFloorPlanDraft(string code, string name)
    {
        return new FloorPlanDraft(code, name, "", "", "", 0, 0, false, "", 0, FloorPlanDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanDraftStatus next)
    {
        switch (Status)
        {
            case FloorPlanDraftStatus.Planned:
                switch (next)
                {
                    case FloorPlanDraftStatus.InProgress:
                    case FloorPlanDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case FloorPlanDraftStatus.Scheduled:
                switch (next)
                {
                    case FloorPlanDraftStatus.InProgress:
                        return true;
                }
                return false;
            case FloorPlanDraftStatus.InProgress:
                return next == FloorPlanDraftStatus.Cancelled;
            case FloorPlanDraftStatus.Completed:
                return next == FloorPlanDraftStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(FloorPlanDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Version * (double)WindowDays, 2);
    }

    public FloorPlanDraft Copy()
    {
        return new FloorPlanDraft(
            Code,
            Name,
            Summary,
            Checksum,
            Team,
            WindowDays,
            Version,
            Pinned,
            Title,
            CreatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
