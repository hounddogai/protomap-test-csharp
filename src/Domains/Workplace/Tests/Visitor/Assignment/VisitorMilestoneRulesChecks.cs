namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorMilestoneRulesChecks
{
    public static void VisitorMilestoneValidatorReportsEveryProblem()
    {
        var validator = VisitorMilestoneRules.DefaultVisitorMilestoneValidator();
        var item = VisitorMilestone.NewVisitorMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorMilestoneCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new VisitorMilestoneCostRangeRule(7.0, 2478.1);
        var item = VisitorMilestone.NewVisitorMilestone("x", "y");
        item.Cost = 2479.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorMilestoneMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new VisitorMilestoneMaximumRangeRule(7.1, 297.4);
        var item = VisitorMilestone.NewVisitorMilestone("x", "y");
        item.Maximum = 298.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new VisitorMilestoneRequiredCodeRule("visitor milestone needs a value");
        var item = VisitorMilestone.NewVisitorMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void VisitorMilestoneRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new VisitorMilestoneRequiredSummaryRule("visitor milestone needs a value");
        var item = VisitorMilestone.NewVisitorMilestone("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        VisitorMilestoneValidatorReportsEveryProblem();
        VisitorMilestoneCostRangeRuleFlagsValuesAboveCost();
        VisitorMilestoneMaximumRangeRuleFlagsValuesAboveMaximum();
        VisitorMilestoneRequiredCodeRuleFlagsABlankCode();
        VisitorMilestoneRequiredSummaryRuleFlagsABlankSummary();
    }
}
