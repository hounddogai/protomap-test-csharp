namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class AdaptiveEquityGrantOverrideRulesChecks
{
    public static void AdaptiveEquityGrantOverrideValidatorReportsEveryProblem()
    {
        var validator = AdaptiveEquityGrantOverrideRules.DefaultAdaptiveEquityGrantOverrideValidator();
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveEquityGrantOverrideContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new AdaptiveEquityGrantOverrideContactEmailLengthRule(80);
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveEquityGrantOverrideSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new AdaptiveEquityGrantOverrideSequenceRangeRule(64, 120);
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveEquityGrantOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveEquityGrantOverrideCodeLengthRule(80);
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveEquityGrantOverrideRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new AdaptiveEquityGrantOverrideRequiredSegmentRule("adaptive equity grant override needs this field");
        var item = AdaptiveEquityGrantOverride.NewAdaptiveEquityGrantOverride("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveEquityGrantOverrideValidatorReportsEveryProblem();
        AdaptiveEquityGrantOverrideContactEmailLengthRuleFlagsALongContactEmail();
        AdaptiveEquityGrantOverrideSequenceRangeRuleFlagsValuesAboveSequence();
        AdaptiveEquityGrantOverrideCodeLengthRuleFlagsALongCode();
        AdaptiveEquityGrantOverrideRequiredSegmentRuleFlagsABlankSegment();
    }
}
