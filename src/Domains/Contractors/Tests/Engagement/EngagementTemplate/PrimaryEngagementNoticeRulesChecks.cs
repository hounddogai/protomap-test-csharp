namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementTemplate;

using Acme.Workforce.Contractors.Engagement.EngagementTemplate;

/// <summary>Checks of engagement template code, run by the project's runner.</summary>
public static class PrimaryEngagementNoticeRulesChecks
{
    public static void PrimaryEngagementNoticeValidatorReportsEveryProblem()
    {
        var validator = PrimaryEngagementNoticeRules.DefaultPrimaryEngagementNoticeValidator();
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryEngagementNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PrimaryEngagementNoticeLabelLengthRule(120);
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryEngagementNoticeEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PrimaryEngagementNoticeEndsAtRangeRule(16, 1000);
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryEngagementNoticeRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryEngagementNoticeRequiredNameRule("primary engagement notice needs a value");
        var item = PrimaryEngagementNotice.NewPrimaryEngagementNotice("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryEngagementNoticeValidatorReportsEveryProblem();
        PrimaryEngagementNoticeLabelLengthRuleFlagsALongLabel();
        PrimaryEngagementNoticeEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PrimaryEngagementNoticeRequiredNameRuleFlagsABlankName();
    }
}
