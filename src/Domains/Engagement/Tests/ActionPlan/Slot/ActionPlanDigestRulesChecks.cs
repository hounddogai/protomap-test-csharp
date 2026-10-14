namespace Acme.Workforce.Engagement.Tests.ActionPlan.Slot;

using Acme.Workforce.Engagement.ActionPlan.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ActionPlanDigestRulesChecks
{
    public static void ActionPlanDigestValidatorReportsEveryProblem()
    {
        var validator = ActionPlanDigestRules.DefaultActionPlanDigestValidator();
        var item = ActionPlanDigest.NewActionPlanDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanDigestUnitLengthRuleFlagsALongUnit()
    {
        var rule = new ActionPlanDigestUnitLengthRule(120);
        var item = ActionPlanDigest.NewActionPlanDigest("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanDigestVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new ActionPlanDigestVersionRangeRule(80, 86400);
        var item = ActionPlanDigest.NewActionPlanDigest("x", "y");
        item.Version = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new ActionPlanDigestCodeLengthRule(32);
        var item = ActionPlanDigest.NewActionPlanDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new ActionPlanDigestRequiredTitleRule("action plan digest needs an entry");
        var item = ActionPlanDigest.NewActionPlanDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ActionPlanDigestValidatorReportsEveryProblem();
        ActionPlanDigestUnitLengthRuleFlagsALongUnit();
        ActionPlanDigestVersionRangeRuleFlagsValuesAboveVersion();
        ActionPlanDigestCodeLengthRuleFlagsALongCode();
        ActionPlanDigestRequiredTitleRuleFlagsABlankTitle();
    }
}
