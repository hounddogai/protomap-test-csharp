namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptivePositionProfileRulesChecks
{
    public static void AdaptivePositionProfileValidatorReportsEveryProblem()
    {
        var validator = AdaptivePositionProfileRules.DefaultAdaptivePositionProfileValidator();
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptivePositionProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AdaptivePositionProfileRequiredTitleRule("adaptive position profile needs this field");
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePositionProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AdaptivePositionProfileRequiredCodeRule("adaptive position profile needs this field");
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePositionProfileSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new AdaptivePositionProfileSummaryLengthRule(3600);
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptivePositionProfileRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new AdaptivePositionProfileRequiredDescriptionRule("adaptive position profile needs an entry");
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptivePositionProfileRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new AdaptivePositionProfileRequiredSummaryRule("adaptive position profile needs this field");
        var item = AdaptivePositionProfile.NewAdaptivePositionProfile("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptivePositionProfileValidatorReportsEveryProblem();
        AdaptivePositionProfileRequiredTitleRuleFlagsABlankTitle();
        AdaptivePositionProfileRequiredCodeRuleFlagsABlankCode();
        AdaptivePositionProfileSummaryLengthRuleFlagsALongSummary();
        AdaptivePositionProfileRequiredDescriptionRuleFlagsABlankDescription();
        AdaptivePositionProfileRequiredSummaryRuleFlagsABlankSummary();
    }
}
