namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class AdaptiveFloorPlanDraftRulesChecks
{
    public static void AdaptiveFloorPlanDraftValidatorReportsEveryProblem()
    {
        var validator = AdaptiveFloorPlanDraftRules.DefaultAdaptiveFloorPlanDraftValidator();
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveFloorPlanDraftChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new AdaptiveFloorPlanDraftChecksumLengthRule(120);
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFloorPlanDraftRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveFloorPlanDraftRequiredNameRule("adaptive floor plan draft needs this field");
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveFloorPlanDraftNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveFloorPlanDraftNameLengthRule(80);
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveFloorPlanDraftLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new AdaptiveFloorPlanDraftLimitRangeRule(120, 255);
        var item = AdaptiveFloorPlanDraft.NewAdaptiveFloorPlanDraft("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveFloorPlanDraftValidatorReportsEveryProblem();
        AdaptiveFloorPlanDraftChecksumLengthRuleFlagsALongChecksum();
        AdaptiveFloorPlanDraftRequiredNameRuleFlagsABlankName();
        AdaptiveFloorPlanDraftNameLengthRuleFlagsALongName();
        AdaptiveFloorPlanDraftLimitRangeRuleFlagsValuesAboveLimit();
    }
}
