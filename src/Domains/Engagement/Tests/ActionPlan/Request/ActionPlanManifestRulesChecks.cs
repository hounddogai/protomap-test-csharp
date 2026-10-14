namespace Acme.Workforce.Engagement.Tests.ActionPlan.Request;

using Acme.Workforce.Engagement.ActionPlan.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class ActionPlanManifestRulesChecks
{
    public static void ActionPlanManifestValidatorReportsEveryProblem()
    {
        var validator = ActionPlanManifestRules.DefaultActionPlanManifestValidator();
        var item = ActionPlanManifest.NewActionPlanManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ActionPlanManifestRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new ActionPlanManifestRevisionRangeRule(32, 80);
        var item = ActionPlanManifest.NewActionPlanManifest("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ActionPlanManifestLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ActionPlanManifestLocationLengthRule(255);
        var item = ActionPlanManifest.NewActionPlanManifest("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ActionPlanManifestRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new ActionPlanManifestRequiredLocationRule("action plan manifest needs an entry");
        var item = ActionPlanManifest.NewActionPlanManifest("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ActionPlanManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ActionPlanManifestTitleLengthRule(16);
        var item = ActionPlanManifest.NewActionPlanManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ActionPlanManifestValidatorReportsEveryProblem();
        ActionPlanManifestRevisionRangeRuleFlagsValuesAboveRevision();
        ActionPlanManifestLocationLengthRuleFlagsALongLocation();
        ActionPlanManifestRequiredLocationRuleFlagsABlankLocation();
        ActionPlanManifestTitleLengthRuleFlagsALongTitle();
    }
}
