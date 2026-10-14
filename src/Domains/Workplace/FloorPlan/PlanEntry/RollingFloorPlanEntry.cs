namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling floor plan entry is in its life cycle.</summary>
public enum RollingFloorPlanEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a rolling floor plan entry and its current status.</summary>
public sealed class RollingFloorPlanEntry(string code, string label, string salaryBand, string timezone, double maximum, string grade, int endsAt, string name, string country, string category, RollingFloorPlanEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Timezone { get; set; } = timezone;
    public double Maximum { get; set; } = maximum;
    public string Grade { get; set; } = grade;
    public int EndsAt { get; set; } = endsAt;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public RollingFloorPlanEntryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling floor plan entry in status proposed.</summary>
    public static RollingFloorPlanEntry NewRollingFloorPlanEntry(string code, string label)
    {
        return new RollingFloorPlanEntry(
            code,
            label,
            "",
            "",
            0.0,
            "",
            0,
            "",
            "",
            "",
            RollingFloorPlanEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingFloorPlanEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingFloorPlanEntryStatus next)
    {
        switch (Status)
        {
            case RollingFloorPlanEntryStatus.Proposed:
                switch (next)
                {
                    case RollingFloorPlanEntryStatus.Approved:
                        return true;
                }
                return false;
            case RollingFloorPlanEntryStatus.Approved:
                return next == RollingFloorPlanEntryStatus.Rejected;
            case RollingFloorPlanEntryStatus.Rejected:
                switch (next)
                {
                    case RollingFloorPlanEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingFloorPlanEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling floor plan entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"rolling floor plan entry"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Maximum + (double)EndsAt, 2);
    }

    public RollingFloorPlanEntry Copy()
    {
        return new RollingFloorPlanEntry(
            Code,
            Label,
            SalaryBand,
            Timezone,
            Maximum,
            Grade,
            EndsAt,
            Name,
            Country,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
