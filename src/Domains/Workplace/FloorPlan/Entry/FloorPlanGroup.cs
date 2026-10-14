namespace Acme.Workforce.Workplace.FloorPlan.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan group is in its life cycle.</summary>
public enum FloorPlanGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a floor plan group and its current status.</summary>
public sealed class FloorPlanGroup(string code, string label, string reviewer, double rate, string country, string locale, string costCenter, FloorPlanGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public double Rate { get; set; } = rate;
    public string Country { get; set; } = country;
    public string Locale { get; set; } = locale;
    public string CostCenter { get; set; } = costCenter;
    public FloorPlanGroupStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan group in status created.</summary>
    public static FloorPlanGroup NewFloorPlanGroup(string code, string label)
    {
        return new FloorPlanGroup(code, label, "", 0.0, "", "", "", FloorPlanGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanGroupStatus next)
    {
        switch (Status)
        {
            case FloorPlanGroupStatus.Created:
                return next == FloorPlanGroupStatus.Published || next == FloorPlanGroupStatus.Retired;
            case FloorPlanGroupStatus.Validated:
                switch (next)
                {
                    case FloorPlanGroupStatus.Retired:
                        return true;
                }
                return false;
            case FloorPlanGroupStatus.Published:
                return next == FloorPlanGroupStatus.Retired;
        }
        return false;
    }

    public void MoveTo(FloorPlanGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public FloorPlanGroup Copy()
    {
        return new FloorPlanGroup(Code, Label, Reviewer, Rate, Country, Locale, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
