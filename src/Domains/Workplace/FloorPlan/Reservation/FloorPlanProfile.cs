namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan profile is in its life cycle.</summary>
public enum FloorPlanProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a floor plan profile and its current status.</summary>
public sealed class FloorPlanProfile(string code, string name, List<string> tags, int durationMinutes, string locale, int limit, string grade, string unit, string description, double weight, double minimum, FloorPlanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Locale { get; set; } = locale;
    public int Limit { get; set; } = limit;
    public string Grade { get; set; } = grade;
    public string Unit { get; set; } = unit;
    public string Description { get; set; } = description;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public FloorPlanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan profile in status draft.</summary>
    public static FloorPlanProfile NewFloorPlanProfile(string code, string name)
    {
        return new FloorPlanProfile(
            code,
            name,
            new List<string>(),
            0,
            "",
            0,
            "",
            "",
            "",
            0.0,
            0.0,
            FloorPlanProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanProfileStatus next)
    {
        switch (Status)
        {
            case FloorPlanProfileStatus.Draft:
                switch (next)
                {
                    case FloorPlanProfileStatus.Pending:
                    case FloorPlanProfileStatus.Suspended:
                        return true;
                }
                return false;
            case FloorPlanProfileStatus.Pending:
                switch (next)
                {
                    case FloorPlanProfileStatus.Suspended:
                        return true;
                }
                return false;
            case FloorPlanProfileStatus.Active:
                return next == FloorPlanProfileStatus.Suspended || next == FloorPlanProfileStatus.Closed;
            case FloorPlanProfileStatus.Suspended:
                return next == FloorPlanProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FloorPlanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
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

    public double WeightedLoad()
    {
        return Math.Round((double)Limit - (double)DurationMinutes, 2);
    }

    public FloorPlanProfile Copy()
    {
        return new FloorPlanProfile(
            Code,
            Name,
            Tags,
            DurationMinutes,
            Locale,
            Limit,
            Grade,
            Unit,
            Description,
            Weight,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
