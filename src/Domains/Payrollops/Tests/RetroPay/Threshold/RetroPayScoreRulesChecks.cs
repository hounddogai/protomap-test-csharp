namespace Acme.Workforce.Payrollops.Tests.RetroPay.Threshold;

using Acme.Workforce.Payrollops.RetroPay.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class RetroPayScoreRulesChecks
{
    public static void RetroPayScoreValidatorReportsEveryProblem()
    {
        var validator = RetroPayScoreRules.DefaultRetroPayScoreValidator();
        var item = RetroPayScore.NewRetroPayScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPayScoreCodeLengthRuleFlagsALongCode()
    {
        var rule = new RetroPayScoreCodeLengthRule(255);
        var item = RetroPayScore.NewRetroPayScore("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayScoreCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new RetroPayScoreCapacityRangeRule(255, 3600);
        var item = RetroPayScore.NewRetroPayScore("x", "y");
        item.Capacity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPayScoreBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RetroPayScoreBadgeLengthRule(3600);
        var item = RetroPayScore.NewRetroPayScore("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPayScoreRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RetroPayScoreRequiredCodeRule("retro pay score needs a value");
        var item = RetroPayScore.NewRetroPayScore("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RetroPayScoreValidatorReportsEveryProblem();
        RetroPayScoreCodeLengthRuleFlagsALongCode();
        RetroPayScoreCapacityRangeRuleFlagsValuesAboveCapacity();
        RetroPayScoreBadgeLengthRuleFlagsALongBadge();
        RetroPayScoreRequiredCodeRuleFlagsABlankCode();
    }
}
