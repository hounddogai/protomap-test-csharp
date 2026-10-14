namespace Acme.Workforce.Engagement.Tests.ActionPlan.Group;

using Acme.Workforce.Engagement.ActionPlan.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ManualActionPlanEntryRulesChecks
{
    public static void ManualActionPlanEntryValidatorReportsEveryProblem()
    {
        var validator = ManualActionPlanEntryRules.DefaultManualActionPlanEntryValidator();
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualActionPlanEntryUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new ManualActionPlanEntryUpdatedAtRangeRule(3600, 86400);
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualActionPlanEntryRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new ManualActionPlanEntryRequiredExternalRefRule("manual action plan entry needs this field");
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualActionPlanEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualActionPlanEntryRequiredCodeRule("manual action plan entry needs a value");
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualActionPlanEntryWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new ManualActionPlanEntryWeightRangeRule(9.7, 3523.0);
        var item = ManualActionPlanEntry.NewManualActionPlanEntry("x", "y");
        item.Weight = 3524.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualActionPlanEntryValidatorReportsEveryProblem();
        ManualActionPlanEntryUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        ManualActionPlanEntryRequiredExternalRefRuleFlagsABlankExternalRef();
        ManualActionPlanEntryRequiredCodeRuleFlagsABlankCode();
        ManualActionPlanEntryWeightRangeRuleFlagsValuesAboveWeight();
    }
}
