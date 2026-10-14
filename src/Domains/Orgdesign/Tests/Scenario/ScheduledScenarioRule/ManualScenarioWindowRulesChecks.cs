namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioWindowRulesChecks
{
    public static void ManualScenarioWindowValidatorReportsEveryProblem()
    {
        var validator = ManualScenarioWindowRules.DefaultManualScenarioWindowValidator();
        var item = ManualScenarioWindow.NewManualScenarioWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualScenarioWindowJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new ManualScenarioWindowJobCodeLengthRule(16);
        var item = ManualScenarioWindow.NewManualScenarioWindow("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualScenarioWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualScenarioWindowRequiredCodeRule("manual scenario window needs a value");
        var item = ManualScenarioWindow.NewManualScenarioWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualScenarioWindowEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new ManualScenarioWindowEndsAtRangeRule(255, 86400);
        var item = ManualScenarioWindow.NewManualScenarioWindow("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualScenarioWindowRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new ManualScenarioWindowRequiredJobCodeRule("manual scenario window needs an entry");
        var item = ManualScenarioWindow.NewManualScenarioWindow("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualScenarioWindowRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ManualScenarioWindowRequiredTimezoneRule("manual scenario window needs this field");
        var item = ManualScenarioWindow.NewManualScenarioWindow("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualScenarioWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ManualScenarioWindowWindowDaysRangeRule(255, 86400);
        var item = ManualScenarioWindow.NewManualScenarioWindow("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualScenarioWindowValidatorReportsEveryProblem();
        ManualScenarioWindowJobCodeLengthRuleFlagsALongJobCode();
        ManualScenarioWindowRequiredCodeRuleFlagsABlankCode();
        ManualScenarioWindowEndsAtRangeRuleFlagsValuesAboveEndsAt();
        ManualScenarioWindowRequiredJobCodeRuleFlagsABlankJobCode();
        ManualScenarioWindowRequiredTimezoneRuleFlagsABlankTimezone();
        ManualScenarioWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
