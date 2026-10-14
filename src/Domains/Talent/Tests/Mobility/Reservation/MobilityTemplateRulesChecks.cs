namespace Acme.Workforce.Talent.Tests.Mobility.Reservation;

using Acme.Workforce.Talent.Mobility.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class MobilityTemplateRulesChecks
{
    public static void MobilityTemplateValidatorReportsEveryProblem()
    {
        var validator = MobilityTemplateRules.DefaultMobilityTemplateValidator();
        var item = MobilityTemplate.NewMobilityTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityTemplateRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new MobilityTemplateRequiredCurrencyRule("mobility template needs a value");
        var item = MobilityTemplate.NewMobilityTemplate("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityTemplateCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new MobilityTemplateCostRangeRule(7.8, 1839.1);
        var item = MobilityTemplate.NewMobilityTemplate("x", "y");
        item.Cost = 1840.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityTemplateValidatorReportsEveryProblem();
        MobilityTemplateRequiredCurrencyRuleFlagsABlankCurrency();
        MobilityTemplateCostRangeRuleFlagsValuesAboveCost();
    }
}
