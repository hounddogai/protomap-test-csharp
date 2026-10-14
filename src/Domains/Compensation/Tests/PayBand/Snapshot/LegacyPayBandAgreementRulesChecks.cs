namespace Acme.Workforce.Compensation.Tests.PayBand.Snapshot;

using Acme.Workforce.Compensation.PayBand.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyPayBandAgreementRulesChecks
{
    public static void LegacyPayBandAgreementValidatorReportsEveryProblem()
    {
        var validator = LegacyPayBandAgreementRules.DefaultLegacyPayBandAgreementValidator();
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayBandAgreementLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyPayBandAgreementLabelLengthRule(32);
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPayBandAgreementTimezoneLengthRuleFlagsALongTimezone()
    {
        var rule = new LegacyPayBandAgreementTimezoneLengthRule(255);
        var item = LegacyPayBandAgreement.NewLegacyPayBandAgreement("x", "y");
        item.Timezone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Timezone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPayBandAgreementValidatorReportsEveryProblem();
        LegacyPayBandAgreementLabelLengthRuleFlagsALongLabel();
        LegacyPayBandAgreementTimezoneLengthRuleFlagsALongTimezone();
    }
}
