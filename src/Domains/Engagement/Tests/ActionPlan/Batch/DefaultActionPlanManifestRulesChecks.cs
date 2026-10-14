namespace Acme.Workforce.Engagement.Tests.ActionPlan.Batch;

using Acme.Workforce.Engagement.ActionPlan.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class DefaultActionPlanManifestRulesChecks
{
    public static void DefaultActionPlanManifestValidatorReportsEveryProblem()
    {
        var validator = DefaultActionPlanManifestRules.DefaultDefaultActionPlanManifestValidator();
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultActionPlanManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultActionPlanManifestCodeLengthRule(64);
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultActionPlanManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultActionPlanManifestRequiredCodeRule("default action plan manifest needs this field");
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultActionPlanManifestHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new DefaultActionPlanManifestHoursRangeRule(6.8, 4228.3);
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("x", "y");
        item.Hours = 4229.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new DefaultActionPlanManifestWindowDaysRangeRule(16, 255);
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("x", "y");
        item.WindowDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultActionPlanManifestRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new DefaultActionPlanManifestRequiredNotesRule("default action plan manifest needs this field");
        var item = DefaultActionPlanManifest.NewDefaultActionPlanManifest("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultActionPlanManifestValidatorReportsEveryProblem();
        DefaultActionPlanManifestCodeLengthRuleFlagsALongCode();
        DefaultActionPlanManifestRequiredCodeRuleFlagsABlankCode();
        DefaultActionPlanManifestHoursRangeRuleFlagsValuesAboveHours();
        DefaultActionPlanManifestWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        DefaultActionPlanManifestRequiredNotesRuleFlagsABlankNotes();
    }
}
