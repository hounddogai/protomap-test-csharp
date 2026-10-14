namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual floor plan item is in its life cycle.</summary>
public enum ManualFloorPlanItemStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual floor plan item as the floor plan team keeps it.</summary>
public sealed class ManualFloorPlanItem(string code, string title, bool archived, int leaveDays, List<string> tags, double hours, string ownerId, ManualFloorPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public int LeaveDays { get; set; } = leaveDays;
    public List<string> Tags { get; set; } = tags;
    public double Hours { get; set; } = hours;
    public string OwnerId { get; set; } = ownerId;
    public ManualFloorPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a manual floor plan item in status planned.</summary>
    public static ManualFloorPlanItem NewManualFloorPlanItem(string code, string title)
    {
        return new ManualFloorPlanItem(
            code,
            title,
            false,
            0,
            new List<string>(),
            0.0,
            "",
            ManualFloorPlanItemStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualFloorPlanItemStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualFloorPlanItemStatus next)
    {
        switch (Status)
        {
            case ManualFloorPlanItemStatus.Planned:
                return next == ManualFloorPlanItemStatus.Scheduled;
            case ManualFloorPlanItemStatus.Scheduled:
                switch (next)
                {
                    case ManualFloorPlanItemStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualFloorPlanItemStatus.InProgress:
                return next == ManualFloorPlanItemStatus.Completed;
            case ManualFloorPlanItemStatus.Completed:
                return next == ManualFloorPlanItemStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualFloorPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual floor plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double ProjectedValue()
    {
        return Math.Round((double)LeaveDays + Hours, 2);
    }

    public ManualFloorPlanItem Copy()
    {
        return new ManualFloorPlanItem(Code, Title, Archived, LeaveDays, Tags, Hours, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
