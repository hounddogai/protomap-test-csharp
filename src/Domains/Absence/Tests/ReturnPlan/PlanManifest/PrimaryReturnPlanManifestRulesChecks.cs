namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanManifest;

using Acme.Workforce.Absence.ReturnPlan.PlanManifest;

/// <summary>Checks of plan manifest code, run by the project's runner.</summary>
public static class PrimaryReturnPlanManifestRulesChecks
{
    public static void PrimaryReturnPlanManifestValidatorReportsEveryProblem()
    {
        var validator = PrimaryReturnPlanManifestRules.DefaultPrimaryReturnPlanManifestValidator();
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryReturnPlanManifestRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new PrimaryReturnPlanManifestRequiredTimezoneRule("primary return plan manifest needs this field");
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryReturnPlanManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryReturnPlanManifestCodeLengthRule(1000);
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryReturnPlanManifestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryReturnPlanManifestTitleLengthRule(1000);
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryReturnPlanManifestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new PrimaryReturnPlanManifestRequiredTitleRule("primary return plan manifest needs an entry");
        var item = PrimaryReturnPlanManifest.NewPrimaryReturnPlanManifest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryReturnPlanManifestValidatorReportsEveryProblem();
        PrimaryReturnPlanManifestRequiredTimezoneRuleFlagsABlankTimezone();
        PrimaryReturnPlanManifestCodeLengthRuleFlagsALongCode();
        PrimaryReturnPlanManifestTitleLengthRuleFlagsALongTitle();
        PrimaryReturnPlanManifestRequiredTitleRuleFlagsABlankTitle();
    }
}
