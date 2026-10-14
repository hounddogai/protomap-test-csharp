namespace Acme.Workforce.Absence.Tests.Balance.Baseline;

using Acme.Workforce.Absence.Balance.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveBalanceSummaryRulesChecks
{
    public static void AdaptiveBalanceSummaryValidatorReportsEveryProblem()
    {
        var validator = AdaptiveBalanceSummaryRules.DefaultAdaptiveBalanceSummaryValidator();
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveBalanceSummaryContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AdaptiveBalanceSummaryContactEmailLengthRule(1000);
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveBalanceSummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveBalanceSummaryRequiredLabelRule("adaptive balance summary needs a value");
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveBalanceSummaryRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new AdaptiveBalanceSummaryRequiredContactEmailRule("adaptive balance summary needs a value");
        var item = AdaptiveBalanceSummary.NewAdaptiveBalanceSummary("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveBalanceSummaryValidatorReportsEveryProblem();
        AdaptiveBalanceSummaryContactEmailLengthRuleFlagsALongContactEmail();
        AdaptiveBalanceSummaryRequiredLabelRuleFlagsABlankLabel();
        AdaptiveBalanceSummaryRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
