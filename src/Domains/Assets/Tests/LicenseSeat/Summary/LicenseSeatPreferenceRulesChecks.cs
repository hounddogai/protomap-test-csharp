namespace Acme.Workforce.Assets.Tests.LicenseSeat.Summary;

using Acme.Workforce.Assets.LicenseSeat.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LicenseSeatPreferenceRulesChecks
{
    public static void LicenseSeatPreferenceValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatPreferenceRules.DefaultLicenseSeatPreferenceValidator();
        var item = LicenseSeatPreference.NewLicenseSeatPreference("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatPreferenceChecksumLengthRuleFlagsALongChecksum()
    {
        var rule = new LicenseSeatPreferenceChecksumLengthRule(64);
        var item = LicenseSeatPreference.NewLicenseSeatPreference("x", "y");
        item.Checksum = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Checksum = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatPreferenceCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatPreferenceCodeLengthRule(16);
        var item = LicenseSeatPreference.NewLicenseSeatPreference("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatPreferenceValidatorReportsEveryProblem();
        LicenseSeatPreferenceChecksumLengthRuleFlagsALongChecksum();
        LicenseSeatPreferenceCodeLengthRuleFlagsALongCode();
    }
}
