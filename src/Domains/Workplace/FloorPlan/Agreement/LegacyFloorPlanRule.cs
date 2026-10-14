namespace Acme.Workforce.Workplace.FloorPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy floor plan rule is in its life cycle.</summary>
public enum LegacyFloorPlanRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a legacy floor plan rule and its current status.</summary>
public sealed class LegacyFloorPlanRule(string code, string label, int tenureMonths, string reviewer, double weight, bool active, int windowDays, double discount, string skill, LegacyFloorPlanRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Reviewer { get; set; } = reviewer;
    public double Weight { get; set; } = weight;
    public bool Active { get; set; } = active;
    public int WindowDays { get; set; } = windowDays;
    public double Discount { get; set; } = discount;
    public string Skill { get; set; } = skill;
    public LegacyFloorPlanRuleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy floor plan rule in status created.</summary>
    public static LegacyFloorPlanRule NewLegacyFloorPlanRule(string code, string label)
    {
        return new LegacyFloorPlanRule(
            code,
            label,
            0,
            "",
            0.0,
            false,
            0,
            0.0,
            "",
            LegacyFloorPlanRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyFloorPlanRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyFloorPlanRuleStatus next)
    {
        switch (Status)
        {
            case LegacyFloorPlanRuleStatus.Created:
                switch (next)
                {
                    case LegacyFloorPlanRuleStatus.Retired:
                        return true;
                }
                return false;
            case LegacyFloorPlanRuleStatus.Validated:
                switch (next)
                {
                    case LegacyFloorPlanRuleStatus.Published:
                    case LegacyFloorPlanRuleStatus.Retired:
                        return true;
                }
                return false;
            case LegacyFloorPlanRuleStatus.Published:
                return next == LegacyFloorPlanRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyFloorPlanRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy floor plan rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)TenureMonths - Discount, 2);
    }

    public LegacyFloorPlanRule Copy()
    {
        return new LegacyFloorPlanRule(
            Code,
            Label,
            TenureMonths,
            Reviewer,
            Weight,
            Active,
            WindowDays,
            Discount,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
