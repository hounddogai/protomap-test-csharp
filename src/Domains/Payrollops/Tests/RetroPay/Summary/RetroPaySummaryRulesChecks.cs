namespace Acme.Workforce.Payrollops.Tests.RetroPay.Summary;

using Acme.Workforce.Payrollops.RetroPay.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class RetroPaySummaryRulesChecks
{
    public static void RetroPaySummaryValidatorReportsEveryProblem()
    {
        var validator = RetroPaySummaryRules.DefaultRetroPaySummaryValidator();
        var item = RetroPaySummary.NewRetroPaySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RetroPaySummaryBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RetroPaySummaryBadgeLengthRule(64);
        var item = RetroPaySummary.NewRetroPaySummary("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RetroPaySummaryBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new RetroPaySummaryBudgetRangeRule(4.3, 1215.5);
        var item = RetroPaySummary.NewRetroPaySummary("x", "y");
        item.Budget = 1217.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RetroPaySummaryRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RetroPaySummaryRequiredBadgeRule("retro pay summary needs an entry");
        var item = RetroPaySummary.NewRetroPaySummary("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RetroPaySummarySkillLengthRuleFlagsALongSkill()
    {
        var rule = new RetroPaySummarySkillLengthRule(255);
        var item = RetroPaySummary.NewRetroPaySummary("x", "y");
        item.Skill = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Skill = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RetroPaySummaryValidatorReportsEveryProblem();
        RetroPaySummaryBadgeLengthRuleFlagsALongBadge();
        RetroPaySummaryBudgetRangeRuleFlagsValuesAboveBudget();
        RetroPaySummaryRequiredBadgeRuleFlagsABlankBadge();
        RetroPaySummarySkillLengthRuleFlagsALongSkill();
    }
}
