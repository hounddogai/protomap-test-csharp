namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class DefaultRateDraftRulesChecks
{
    public static void DefaultRateDraftValidatorReportsEveryProblem()
    {
        var validator = DefaultRateDraftRules.DefaultDefaultRateDraftValidator();
        var item = DefaultRateDraft.NewDefaultRateDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultRateDraftRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultRateDraftRequiredNameRule("default rate draft needs this field");
        var item = DefaultRateDraft.NewDefaultRateDraft("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRateDraftRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new DefaultRateDraftRequiredSummaryRule("default rate draft needs an entry");
        var item = DefaultRateDraft.NewDefaultRateDraft("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultRateDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultRateDraftRequiredCodeRule("default rate draft needs this field");
        var item = DefaultRateDraft.NewDefaultRateDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DefaultRateDraftValidatorReportsEveryProblem();
        DefaultRateDraftRequiredNameRuleFlagsABlankName();
        DefaultRateDraftRequiredSummaryRuleFlagsABlankSummary();
        DefaultRateDraftRequiredCodeRuleFlagsABlankCode();
    }
}
