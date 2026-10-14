namespace Acme.Workforce.Workplace.FloorPlan.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a floor plan rule is in its life cycle.</summary>
public enum FloorPlanRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a floor plan rule and its current status.</summary>
public sealed class FloorPlanRule(string code, string label, string jobCode, double discount, string region, string source, FloorPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Discount { get; set; } = discount;
    public string Region { get; set; } = region;
    public string Source { get; set; } = source;
    public FloorPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a floor plan rule in status draft.</summary>
    public static FloorPlanRule NewFloorPlanRule(string code, string label)
    {
        return new FloorPlanRule(code, label, "", 0.0, "", "", FloorPlanRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FloorPlanRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FloorPlanRuleStatus next)
    {
        switch (Status)
        {
            case FloorPlanRuleStatus.Draft:
                return next == FloorPlanRuleStatus.Pending;
            case FloorPlanRuleStatus.Pending:
                switch (next)
                {
                    case FloorPlanRuleStatus.Suspended:
                        return true;
                }
                return false;
            case FloorPlanRuleStatus.Active:
                return next == FloorPlanRuleStatus.Suspended;
            case FloorPlanRuleStatus.Suspended:
                return next == FloorPlanRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FloorPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"floor plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"floor plan rule"} {Code}";
    }

    public double MeasuredValue()
    {
        return Discount;
    }

    public FloorPlanRule Copy()
    {
        return new FloorPlanRule(Code, Label, JobCode, Discount, Region, Source, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
