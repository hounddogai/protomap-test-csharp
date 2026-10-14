namespace Acme.Workforce.Workplace.FloorPlan.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan request is in its life cycle.</summary>
public enum FloorPlanRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A floor plan request as the floor plan team keeps it.</summary>
public sealed class FloorPlanRequest(string code, string label, List<string> tags, double ceiling, bool active, double hours, string skill, Dictionary<string, string> attributes, string team, int startsAt, double ratio, FloorPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Ceiling { get; set; } = ceiling;
    public bool Active { get; set; } = active;
    public double Hours { get; set; } = hours;
    public string Skill { get; set; } = skill;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public int StartsAt { get; set; } = startsAt;
    public double Ratio { get; set; } = ratio;
    public FloorPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan request in status proposed.</summary>
    public static FloorPlanRequest NewFloorPlanRequest(string code, string label)
    {
        return new FloorPlanRequest(
            code,
            label,
            new List<string>(),
            0.0,
            false,
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            FloorPlanRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanRequestStatus next)
    {
        switch (Status)
        {
            case FloorPlanRequestStatus.Proposed:
                return next == FloorPlanRequestStatus.Approved;
            case FloorPlanRequestStatus.Approved:
                return next == FloorPlanRequestStatus.Withdrawn;
            case FloorPlanRequestStatus.Rejected:
                switch (next)
                {
                    case FloorPlanRequestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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
        return Math.Round(Hours - (double)StartsAt, 2);
    }

    public FloorPlanRequest Copy()
    {
        return new FloorPlanRequest(
            Code,
            Label,
            Tags,
            Ceiling,
            Active,
            Hours,
            Skill,
            Attributes,
            Team,
            StartsAt,
            Ratio,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
