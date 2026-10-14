namespace Acme.Workforce.Workplace.FloorPlan.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan window is in its life cycle.</summary>
public enum FloorPlanWindowStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One floor plan window, with what the planners need to act on it.</summary>
public sealed class FloorPlanWindow(string code, string name, string contactEmail, double rating, string country, List<string> tags, string reviewer, FloorPlanWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public double Rating { get; set; } = rating;
    public string Country { get; set; } = country;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public FloorPlanWindowStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan window in status proposed.</summary>
    public static FloorPlanWindow NewFloorPlanWindow(string code, string name)
    {
        return new FloorPlanWindow(code, name, "", 0.0, "", new List<string>(), "", FloorPlanWindowStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanWindowStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanWindowStatus next)
    {
        switch (Status)
        {
            case FloorPlanWindowStatus.Proposed:
                switch (next)
                {
                    case FloorPlanWindowStatus.Approved:
                    case FloorPlanWindowStatus.Rejected:
                        return true;
                }
                return false;
            case FloorPlanWindowStatus.Approved:
                switch (next)
                {
                    case FloorPlanWindowStatus.Withdrawn:
                        return true;
                }
                return false;
            case FloorPlanWindowStatus.Rejected:
                return next == FloorPlanWindowStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FloorPlanWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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
        return Rating;
    }

    public FloorPlanWindow Copy()
    {
        return new FloorPlanWindow(Code, Name, ContactEmail, Rating, Country, Tags, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
