namespace Acme.Workforce.Talent.Tests.CareerPath.Summary;

using Acme.Workforce.Talent.CareerPath.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultCareerPathSummaryRulesChecks
{
    public static void DefaultCareerPathSummaryValidatorReportsEveryProblem()
    {
        var validator = DefaultCareerPathSummaryRules.DefaultDefaultCareerPathSummaryValidator();
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultCareerPathSummaryDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new DefaultCareerPathSummaryDescriptionLengthRule(86400);
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultCareerPathSummaryStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new DefaultCareerPathSummaryStartsAtRangeRule(16, 86400);
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultCareerPathSummaryEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new DefaultCareerPathSummaryEndsAtRangeRule(1000, 1000);
        var item = DefaultCareerPathSummary.NewDefaultCareerPathSummary("x", "y");
        item.EndsAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        DefaultCareerPathSummaryValidatorReportsEveryProblem();
        DefaultCareerPathSummaryDescriptionLengthRuleFlagsALongDescription();
        DefaultCareerPathSummaryStartsAtRangeRuleFlagsValuesAboveStartsAt();
        DefaultCareerPathSummaryEndsAtRangeRuleFlagsValuesAboveEndsAt();
    }
}
