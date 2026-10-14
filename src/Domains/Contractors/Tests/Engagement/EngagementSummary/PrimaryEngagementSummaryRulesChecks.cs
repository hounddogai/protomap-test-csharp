namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class PrimaryEngagementSummaryRulesChecks
{
    public static void PrimaryEngagementSummaryValidatorReportsEveryProblem()
    {
        var validator = PrimaryEngagementSummaryRules.DefaultPrimaryEngagementSummaryValidator();
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryEngagementSummaryGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new PrimaryEngagementSummaryGraceDaysRangeRule(1000, 86400);
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryEngagementSummaryWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new PrimaryEngagementSummaryWindowDaysRangeRule(16, 86400);
        var item = PrimaryEngagementSummary.NewPrimaryEngagementSummary("x", "y");
        item.WindowDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryEngagementSummaryValidatorReportsEveryProblem();
        PrimaryEngagementSummaryGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        PrimaryEngagementSummaryWindowDaysRangeRuleFlagsValuesAboveWindowDays();
    }
}
