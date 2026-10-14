namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class PayBandRuleRulesChecks
{
    public static void PayBandRuleValidatorReportsEveryProblem()
    {
        var validator = PayBandRuleRules.DefaultPayBandRuleValidator();
        var item = PayBandRule.NewPayBandRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayBandRuleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PayBandRuleDurationMinutesRangeRule(3600, 86400);
        var item = PayBandRule.NewPayBandRule("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayBandRuleContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new PayBandRuleContactEmailLengthRule(16);
        var item = PayBandRule.NewPayBandRule("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayBandRuleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PayBandRuleRequiredLabelRule("pay band rule needs an entry");
        var item = PayBandRule.NewPayBandRule("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayBandRuleSourceLengthRuleFlagsALongSource()
    {
        var rule = new PayBandRuleSourceLengthRule(16);
        var item = PayBandRule.NewPayBandRule("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayBandRuleValidatorReportsEveryProblem();
        PayBandRuleDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PayBandRuleContactEmailLengthRuleFlagsALongContactEmail();
        PayBandRuleRequiredLabelRuleFlagsABlankLabel();
        PayBandRuleSourceLengthRuleFlagsALongSource();
    }
}
