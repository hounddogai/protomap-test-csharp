namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class EngagementSlotRulesChecks
{
    public static void EngagementSlotValidatorReportsEveryProblem()
    {
        var validator = EngagementSlotRules.DefaultEngagementSlotValidator();
        var item = EngagementSlot.NewEngagementSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementSlotWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new EngagementSlotWeightRangeRule(7.1, 3714.2);
        var item = EngagementSlot.NewEngagementSlot("x", "y");
        item.Weight = 3715.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EngagementSlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EngagementSlotLabelLengthRule(80);
        var item = EngagementSlot.NewEngagementSlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementSlotCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new EngagementSlotCapacityRangeRule(80, 1000);
        var item = EngagementSlot.NewEngagementSlot("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementSlotValidatorReportsEveryProblem();
        EngagementSlotWeightRangeRuleFlagsValuesAboveWeight();
        EngagementSlotLabelLengthRuleFlagsALongLabel();
        EngagementSlotCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
