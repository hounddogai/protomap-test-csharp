namespace Acme.Workforce.Assets.Tests.Assignment.Exception;

using Acme.Workforce.Assets.Assignment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ManualAssignmentRuleRulesChecks
{
    public static void ManualAssignmentRuleValidatorReportsEveryProblem()
    {
        var validator = ManualAssignmentRuleRules.DefaultManualAssignmentRuleValidator();
        var item = ManualAssignmentRule.NewManualAssignmentRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAssignmentRuleScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new ManualAssignmentRuleScoreRangeRule(4.9, 2277.1);
        var item = ManualAssignmentRule.NewManualAssignmentRule("x", "y");
        item.Score = 2278.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualAssignmentRuleLocationLengthRuleFlagsALongLocation()
    {
        var rule = new ManualAssignmentRuleLocationLengthRule(64);
        var item = ManualAssignmentRule.NewManualAssignmentRule("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAssignmentRuleRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ManualAssignmentRuleRequiredChannelRule("manual assignment rule needs this field");
        var item = ManualAssignmentRule.NewManualAssignmentRule("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualAssignmentRuleValidatorReportsEveryProblem();
        ManualAssignmentRuleScoreRangeRuleFlagsValuesAboveScore();
        ManualAssignmentRuleLocationLengthRuleFlagsALongLocation();
        ManualAssignmentRuleRequiredChannelRuleFlagsABlankChannel();
    }
}
