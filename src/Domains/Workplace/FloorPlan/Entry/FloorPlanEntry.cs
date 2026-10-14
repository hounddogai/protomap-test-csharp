namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan entry is in its life cycle.</summary>
public enum FloorPlanEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One floor plan entry, with what the planners need to act on it.</summary>
public sealed class FloorPlanEntry(string code, string name, double rate, double ratio, string title, string unit, Dictionary<string, string> attributes, int createdAt, int revision, string salaryBand, FloorPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public double Ratio { get; set; } = ratio;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int CreatedAt { get; set; } = createdAt;
    public int Revision { get; set; } = revision;
    public string SalaryBand { get; set; } = salaryBand;
    public FloorPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan entry in status proposed.</summary>
    public static FloorPlanEntry NewFloorPlanEntry(string code, string name)
    {
        return new FloorPlanEntry(
            code,
            name,
            0.0,
            0.0,
            "",
            "",
            new Dictionary<string, string>(),
            0,
            0,
            "",
            FloorPlanEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanEntryStatus next)
    {
        switch (Status)
        {
            case FloorPlanEntryStatus.Proposed:
                switch (next)
                {
                    case FloorPlanEntryStatus.Rejected:
                    case FloorPlanEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case FloorPlanEntryStatus.Approved:
                switch (next)
                {
                    case FloorPlanEntryStatus.Withdrawn:
                        return true;
                }
                return false;
            case FloorPlanEntryStatus.Rejected:
                switch (next)
                {
                    case FloorPlanEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FloorPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double AdjustedLoad()
    {
        return Math.Round((double)CreatedAt - Rate, 2);
    }

    public FloorPlanEntry Copy()
    {
        return new FloorPlanEntry(
            Code,
            Name,
            Rate,
            Ratio,
            Title,
            Unit,
            Attributes,
            CreatedAt,
            Revision,
            SalaryBand,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
