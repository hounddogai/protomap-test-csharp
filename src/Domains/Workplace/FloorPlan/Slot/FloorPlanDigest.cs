namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan digest is in its life cycle.</summary>
public enum FloorPlanDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a floor plan digest and its current status.</summary>
public sealed class FloorPlanDigest(string code, string label, string notes, List<string> tags, int priority, string description, string unit, int retries, string category, FloorPlanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public List<string> Tags { get; set; } = tags;
    public int Priority { get; set; } = priority;
    public string Description { get; set; } = description;
    public string Unit { get; set; } = unit;
    public int Retries { get; set; } = retries;
    public string Category { get; set; } = category;
    public FloorPlanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan digest in status idle.</summary>
    public static FloorPlanDigest NewFloorPlanDigest(string code, string label)
    {
        return new FloorPlanDigest(
            code,
            label,
            "",
            new List<string>(),
            0,
            "",
            "",
            0,
            "",
            FloorPlanDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanDigestStatus next)
    {
        switch (Status)
        {
            case FloorPlanDigestStatus.Idle:
                return next == FloorPlanDigestStatus.Draining || next == FloorPlanDigestStatus.Stopped;
            case FloorPlanDigestStatus.Warming:
                switch (next)
                {
                    case FloorPlanDigestStatus.Draining:
                    case FloorPlanDigestStatus.Stopped:
                        return true;
                }
                return false;
            case FloorPlanDigestStatus.Ready:
                switch (next)
                {
                    case FloorPlanDigestStatus.Stopped:
                        return true;
                }
                return false;
            case FloorPlanDigestStatus.Draining:
                return next == FloorPlanDigestStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FloorPlanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan digest {Code} cannot move from {Status} to {next}");
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

    public double AdjustedVolume()
    {
        return Math.Round((double)Priority + (double)Retries, 2);
    }

    public FloorPlanDigest Copy()
    {
        return new FloorPlanDigest(Code, Label, Notes, Tags, Priority, Description, Unit, Retries, Category, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
