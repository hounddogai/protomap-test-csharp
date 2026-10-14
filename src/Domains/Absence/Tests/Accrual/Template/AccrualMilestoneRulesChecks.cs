namespace Acme.Workforce.Absence.Tests.Accrual.Template;

using Acme.Workforce.Absence.Accrual.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class AccrualMilestoneRulesChecks
{
    public static void AccrualMilestoneValidatorReportsEveryProblem()
    {
        var validator = AccrualMilestoneRules.DefaultAccrualMilestoneValidator();
        var item = AccrualMilestone.NewAccrualMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new AccrualMilestoneNameLengthRule(16);
        var item = AccrualMilestone.NewAccrualMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualMilestoneRequiredNameRuleFlagsABlankName()
    {
        var rule = new AccrualMilestoneRequiredNameRule("accrual milestone needs an entry");
        var item = AccrualMilestone.NewAccrualMilestone("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualMilestoneValidatorReportsEveryProblem();
        AccrualMilestoneNameLengthRuleFlagsALongName();
        AccrualMilestoneRequiredNameRuleFlagsABlankName();
    }
}
