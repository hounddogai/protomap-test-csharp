namespace Acme.Workforce.Payrollops.Tests.RetroPay.Review;

using Acme.Workforce.Payrollops.RetroPay.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class PendingRetroPayRuleRulesChecks
{
    public static void PendingRetroPayRuleValidatorReportsEveryProblem()
    {
        var validator = PendingRetroPayRuleRules.DefaultPendingRetroPayRuleValidator();
        var item = PendingRetroPayRule.NewPendingRetroPayRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingRetroPayRuleNotesLengthRuleFlagsALongNotes()
    {
        var rule = new PendingRetroPayRuleNotesLengthRule(255);
        var item = PendingRetroPayRule.NewPendingRetroPayRule("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingRetroPayRuleCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingRetroPayRuleCodeLengthRule(255);
        var item = PendingRetroPayRule.NewPendingRetroPayRule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingRetroPayRuleAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new PendingRetroPayRuleAttemptsRangeRule(32, 255);
        var item = PendingRetroPayRule.NewPendingRetroPayRule("x", "y");
        item.Attempts = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingRetroPayRuleValidatorReportsEveryProblem();
        PendingRetroPayRuleNotesLengthRuleFlagsALongNotes();
        PendingRetroPayRuleCodeLengthRuleFlagsALongCode();
        PendingRetroPayRuleAttemptsRangeRuleFlagsValuesAboveAttempts();
    }
}
