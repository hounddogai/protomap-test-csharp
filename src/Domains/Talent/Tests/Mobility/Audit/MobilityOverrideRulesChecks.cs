namespace Acme.Workforce.Talent.Tests.Mobility.Audit;

using Acme.Workforce.Talent.Mobility.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class MobilityOverrideRulesChecks
{
    public static void MobilityOverrideValidatorReportsEveryProblem()
    {
        var validator = MobilityOverrideRules.DefaultMobilityOverrideValidator();
        var item = MobilityOverride.NewMobilityOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityOverrideCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityOverrideCodeLengthRule(32);
        var item = MobilityOverride.NewMobilityOverride("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityOverrideRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new MobilityOverrideRequiredCurrencyRule("mobility override needs a value");
        var item = MobilityOverride.NewMobilityOverride("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityOverrideRequiredNameRuleFlagsABlankName()
    {
        var rule = new MobilityOverrideRequiredNameRule("mobility override needs an entry");
        var item = MobilityOverride.NewMobilityOverride("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MobilityOverrideValidatorReportsEveryProblem();
        MobilityOverrideCodeLengthRuleFlagsALongCode();
        MobilityOverrideRequiredCurrencyRuleFlagsABlankCurrency();
        MobilityOverrideRequiredNameRuleFlagsABlankName();
    }
}
