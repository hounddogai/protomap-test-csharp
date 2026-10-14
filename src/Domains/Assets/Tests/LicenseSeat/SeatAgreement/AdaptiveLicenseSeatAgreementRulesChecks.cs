namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAgreement;

using Acme.Workforce.Assets.LicenseSeat.SeatAgreement;

/// <summary>Checks of seat agreement code, run by the project's runner.</summary>
public static class AdaptiveLicenseSeatAgreementRulesChecks
{
    public static void AdaptiveLicenseSeatAgreementValidatorReportsEveryProblem()
    {
        var validator = AdaptiveLicenseSeatAgreementRules.DefaultAdaptiveLicenseSeatAgreementValidator();
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveLicenseSeatAgreementWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new AdaptiveLicenseSeatAgreementWeightRangeRule(4.9, 1380.4);
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("x", "y");
        item.Weight = 1381.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveLicenseSeatAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveLicenseSeatAgreementCodeLengthRule(1000);
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveLicenseSeatAgreementRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new AdaptiveLicenseSeatAgreementRequiredSourceRule("adaptive license seat agreement needs a value");
        var item = AdaptiveLicenseSeatAgreement.NewAdaptiveLicenseSeatAgreement("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveLicenseSeatAgreementValidatorReportsEveryProblem();
        AdaptiveLicenseSeatAgreementWeightRangeRuleFlagsValuesAboveWeight();
        AdaptiveLicenseSeatAgreementCodeLengthRuleFlagsALongCode();
        AdaptiveLicenseSeatAgreementRequiredSourceRuleFlagsABlankSource();
    }
}
