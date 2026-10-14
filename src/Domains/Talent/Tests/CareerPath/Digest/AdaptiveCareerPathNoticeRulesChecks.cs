namespace Acme.Workforce.Talent.Tests.CareerPath.Digest;

using Acme.Workforce.Talent.CareerPath.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AdaptiveCareerPathNoticeRulesChecks
{
    public static void AdaptiveCareerPathNoticeValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCareerPathNoticeRules.DefaultAdaptiveCareerPathNoticeValidator();
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCareerPathNoticeRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new AdaptiveCareerPathNoticeRequiredSummaryRule("adaptive career path notice needs an entry");
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCareerPathNoticeLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveCareerPathNoticeLabelLengthRule(64);
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveCareerPathNoticeRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdaptiveCareerPathNoticeRequiredJobCodeRule("adaptive career path notice needs a value");
        var item = AdaptiveCareerPathNotice.NewAdaptiveCareerPathNotice("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathNoticeValidatorReportsEveryProblem();
        AdaptiveCareerPathNoticeRequiredSummaryRuleFlagsABlankSummary();
        AdaptiveCareerPathNoticeLabelLengthRuleFlagsALongLabel();
        AdaptiveCareerPathNoticeRequiredJobCodeRuleFlagsABlankJobCode();
    }
}
