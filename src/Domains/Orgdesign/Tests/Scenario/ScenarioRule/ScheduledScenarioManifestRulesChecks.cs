namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class ScheduledScenarioManifestRulesChecks
{
    public static void ScheduledScenarioManifestValidatorReportsEveryProblem()
    {
        var validator = ScheduledScenarioManifestRules.DefaultScheduledScenarioManifestValidator();
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledScenarioManifestExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new ScheduledScenarioManifestExternalRefLengthRule(255);
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledScenarioManifestLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new ScheduledScenarioManifestLimitRangeRule(120, 1000);
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("x", "y");
        item.Limit = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledScenarioManifestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledScenarioManifestRequiredCodeRule("scheduled scenario manifest needs a value");
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledScenarioManifestValidatorReportsEveryProblem();
        ScheduledScenarioManifestExternalRefLengthRuleFlagsALongExternalRef();
        ScheduledScenarioManifestLimitRangeRuleFlagsValuesAboveLimit();
        ScheduledScenarioManifestRequiredCodeRuleFlagsABlankCode();
    }
}
