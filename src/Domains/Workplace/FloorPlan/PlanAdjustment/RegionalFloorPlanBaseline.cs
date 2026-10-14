namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional floor plan baseline is in its life cycle.</summary>
public enum RegionalFloorPlanBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A regional floor plan baseline as the floor plan team keeps it.</summary>
public sealed class RegionalFloorPlanBaseline(string code, string label, List<string> tags, double hours, double margin, double score, RegionalFloorPlanBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public List<string> Tags { get; set; } = tags;
    public double Hours { get; set; } = hours;
    public double Margin { get; set; } = margin;
    public double Score { get; set; } = score;
    public RegionalFloorPlanBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a regional floor plan baseline in status proposed.</summary>
    public static RegionalFloorPlanBaseline NewRegionalFloorPlanBaseline(string code, string label)
    {
        return new RegionalFloorPlanBaseline(
            code,
            label,
            new List<string>(),
            0.0,
            0.0,
            0.0,
            RegionalFloorPlanBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalFloorPlanBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalFloorPlanBaselineStatus next)
    {
        switch (Status)
        {
            case RegionalFloorPlanBaselineStatus.Proposed:
                switch (next)
                {
                    case RegionalFloorPlanBaselineStatus.Rejected:
                        return true;
                }
                return false;
            case RegionalFloorPlanBaselineStatus.Approved:
                switch (next)
                {
                    case RegionalFloorPlanBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case RegionalFloorPlanBaselineStatus.Rejected:
                return next == RegionalFloorPlanBaselineStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(RegionalFloorPlanBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional floor plan baseline {Code} cannot move from {Status} to {next}");
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

    public double WeightedExposure()
    {
        return Math.Round(Margin - Hours, 2);
    }

    public RegionalFloorPlanBaseline Copy()
    {
        return new RegionalFloorPlanBaseline(Code, Label, Tags, Hours, Margin, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
