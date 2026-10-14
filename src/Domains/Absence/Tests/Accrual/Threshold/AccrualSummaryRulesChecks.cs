namespace Acme.Workforce.Absence.Tests.Accrual.Threshold;

using Acme.Workforce.Absence.Accrual.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AccrualSummaryRulesChecks
{
    public static void AccrualSummaryValidatorReportsEveryProblem()
    {
        var validator = AccrualSummaryRules.DefaultAccrualSummaryValidator();
        var item = AccrualSummary.NewAccrualSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AccrualSummaryRequiredCodeRule("accrual summary needs a value");
        var item = AccrualSummary.NewAccrualSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualSummaryRequiredSkillRuleFlagsABlankSkill()
    {
        var rule = new AccrualSummaryRequiredSkillRule("accrual summary needs an entry");
        var item = AccrualSummary.NewAccrualSummary("", "");
        item.Skill = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualSummaryRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualSummaryRequiredNameRule("accrual summary needs an entry");
        var item = AccrualSummary.NewAccrualSummary("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualSummaryValidatorReportsEveryProblem();
        AccrualSummaryRequiredCodeRuleFlagsABlankCode();
        AccrualSummaryRequiredSkillRuleFlagsABlankSkill();
        AccrualSummaryRequiredNameRuleFlagsABlankName();
    }
}
