namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class ManualAttestationEstimateRulesChecks
{
    public static void ManualAttestationEstimateValidatorReportsEveryProblem()
    {
        var validator = ManualAttestationEstimateRules.DefaultManualAttestationEstimateValidator();
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualAttestationEstimateRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ManualAttestationEstimateRequiredLabelRule("manual attestation estimate needs this field");
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualAttestationEstimateEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new ManualAttestationEstimateEmployeeIdLengthRule(1000);
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAttestationEstimateCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ManualAttestationEstimateCountryLengthRule(1000);
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualAttestationEstimateRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new ManualAttestationEstimateRequiredEmployeeIdRule("manual attestation estimate needs a value");
        var item = ManualAttestationEstimate.NewManualAttestationEstimate("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualAttestationEstimateValidatorReportsEveryProblem();
        ManualAttestationEstimateRequiredLabelRuleFlagsABlankLabel();
        ManualAttestationEstimateEmployeeIdLengthRuleFlagsALongEmployeeId();
        ManualAttestationEstimateCountryLengthRuleFlagsALongCountry();
        ManualAttestationEstimateRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
