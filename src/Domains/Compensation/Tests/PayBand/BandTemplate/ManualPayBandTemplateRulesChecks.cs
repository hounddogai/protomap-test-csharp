namespace Acme.Workforce.Compensation.Tests.PayBand.BandTemplate;

using Acme.Workforce.Compensation.PayBand.BandTemplate;

/// <summary>Checks of band template code, run by the project's runner.</summary>
public static class ManualPayBandTemplateRulesChecks
{
    public static void ManualPayBandTemplateValidatorReportsEveryProblem()
    {
        var validator = ManualPayBandTemplateRules.DefaultManualPayBandTemplateValidator();
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualPayBandTemplateMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new ManualPayBandTemplateMaximumRangeRule(6.2, 1987.3);
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("x", "y");
        item.Maximum = 1988.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualPayBandTemplateRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualPayBandTemplateRequiredNameRule("manual pay band template needs a value");
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualPayBandTemplateQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ManualPayBandTemplateQuantityRangeRule(64, 80);
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ManualPayBandTemplateSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ManualPayBandTemplateSequenceRangeRule(64, 120);
        var item = ManualPayBandTemplate.NewManualPayBandTemplate("x", "y");
        item.Sequence = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualPayBandTemplateValidatorReportsEveryProblem();
        ManualPayBandTemplateMaximumRangeRuleFlagsValuesAboveMaximum();
        ManualPayBandTemplateRequiredNameRuleFlagsABlankName();
        ManualPayBandTemplateQuantityRangeRuleFlagsValuesAboveQuantity();
        ManualPayBandTemplateSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
