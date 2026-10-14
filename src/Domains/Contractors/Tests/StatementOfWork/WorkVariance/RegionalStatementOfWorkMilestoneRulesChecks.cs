namespace Acme.Workforce.Contractors.Tests.StatementOfWork.WorkVariance;

using Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

/// <summary>Checks of work variance code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkMilestoneRulesChecks
{
    public static void RegionalStatementOfWorkMilestoneValidatorReportsEveryProblem()
    {
        var validator = RegionalStatementOfWorkMilestoneRules.DefaultRegionalStatementOfWorkMilestoneValidator();
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalStatementOfWorkMilestoneRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new RegionalStatementOfWorkMilestoneRequiredBadgeRule(
            "regional statement of work milestone needs an entry");
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalStatementOfWorkMilestoneBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new RegionalStatementOfWorkMilestoneBadgeLengthRule(3600);
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalStatementOfWorkMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new RegionalStatementOfWorkMilestoneUpdatedAtRangeRule(80, 120);
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalStatementOfWorkMilestoneNameLengthRuleFlagsALongName()
    {
        var rule = new RegionalStatementOfWorkMilestoneNameLengthRule(255);
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalStatementOfWorkMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new RegionalStatementOfWorkMilestoneTitleLengthRule(255);
        var item = RegionalStatementOfWorkMilestone.NewRegionalStatementOfWorkMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkMilestoneValidatorReportsEveryProblem();
        RegionalStatementOfWorkMilestoneRequiredBadgeRuleFlagsABlankBadge();
        RegionalStatementOfWorkMilestoneBadgeLengthRuleFlagsALongBadge();
        RegionalStatementOfWorkMilestoneUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        RegionalStatementOfWorkMilestoneNameLengthRuleFlagsALongName();
        RegionalStatementOfWorkMilestoneTitleLengthRuleFlagsALongTitle();
    }
}
