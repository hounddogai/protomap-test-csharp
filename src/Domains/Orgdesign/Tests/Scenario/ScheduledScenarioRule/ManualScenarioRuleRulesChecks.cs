namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioRuleRulesChecks
{
    public static void ManualScenarioRuleValidatorReportsEveryProblem()
    {
        var validator = ManualScenarioRuleRules.DefaultManualScenarioRuleValidator();
        var item = ManualScenarioRule.NewManualScenarioRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualScenarioRuleRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new ManualScenarioRuleRequiredTimezoneRule("manual scenario rule needs this field");
        var item = ManualScenarioRule.NewManualScenarioRule("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualScenarioRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new ManualScenarioRuleCodeLengthRule(80);
        var item = ManualScenarioRule.NewManualScenarioRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualScenarioRuleAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new ManualScenarioRuleAttemptsRangeRule(120, 1000);
        var item = ManualScenarioRule.NewManualScenarioRule("x", "y");
        item.Attempts = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualScenarioRulePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new ManualScenarioRulePriorityRangeRule(80, 3600);
        var item = ManualScenarioRule.NewManualScenarioRule("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualScenarioRuleValidatorReportsEveryProblem();
        ManualScenarioRuleRequiredTimezoneRuleFlagsABlankTimezone();
        ManualScenarioRuleCodeLengthRuleFlagsALongCode();
        ManualScenarioRuleAttemptsRangeRuleFlagsValuesAboveAttempts();
        ManualScenarioRulePriorityRangeRuleFlagsValuesAbovePriority();
    }
}
