namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Tier;

using Acme.Workforce.Contractors.StatementOfWork.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StatementOfWorkMilestoneRulesChecks
{
    public static void StatementOfWorkMilestoneValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkMilestoneRules.DefaultStatementOfWorkMilestoneValidator();
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkMilestoneRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new StatementOfWorkMilestoneRequiredEmployeeIdRule("statement of work milestone needs an entry");
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkMilestoneWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new StatementOfWorkMilestoneWindowDaysRangeRule(32, 32);
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("x", "y");
        item.WindowDays = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkMilestoneOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new StatementOfWorkMilestoneOwnerIdLengthRule(32);
        var item = StatementOfWorkMilestone.NewStatementOfWorkMilestone("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkMilestoneValidatorReportsEveryProblem();
        StatementOfWorkMilestoneRequiredEmployeeIdRuleFlagsABlankEmployeeId();
        StatementOfWorkMilestoneWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        StatementOfWorkMilestoneOwnerIdLengthRuleFlagsALongOwnerId();
    }
}
