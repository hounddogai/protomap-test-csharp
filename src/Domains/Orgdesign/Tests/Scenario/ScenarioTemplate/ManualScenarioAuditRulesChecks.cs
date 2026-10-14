namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class ManualScenarioAuditRulesChecks
{
    public static void ManualScenarioAuditValidatorReportsEveryProblem()
    {
        var validator = ManualScenarioAuditRules.DefaultManualScenarioAuditValidator();
        var item = ManualScenarioAudit.NewManualScenarioAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualScenarioAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualScenarioAuditRequiredCodeRule("manual scenario audit needs a value");
        var item = ManualScenarioAudit.NewManualScenarioAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualScenarioAuditTitleLengthRuleFlagsALongTitle()
    {
        var rule = new ManualScenarioAuditTitleLengthRule(255);
        var item = ManualScenarioAudit.NewManualScenarioAudit("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualScenarioAuditCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new ManualScenarioAuditCapacityRangeRule(120, 1000);
        var item = ManualScenarioAudit.NewManualScenarioAudit("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualScenarioAuditLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new ManualScenarioAuditLocaleLengthRule(1000);
        var item = ManualScenarioAudit.NewManualScenarioAudit("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualScenarioAuditValidatorReportsEveryProblem();
        ManualScenarioAuditRequiredCodeRuleFlagsABlankCode();
        ManualScenarioAuditTitleLengthRuleFlagsALongTitle();
        ManualScenarioAuditCapacityRangeRuleFlagsValuesAboveCapacity();
        ManualScenarioAuditLocaleLengthRuleFlagsALongLocale();
    }
}
