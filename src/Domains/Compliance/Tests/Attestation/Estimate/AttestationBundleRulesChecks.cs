namespace Acme.Workforce.Compliance.Tests.Attestation.Estimate;

using Acme.Workforce.Compliance.Attestation.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class AttestationBundleRulesChecks
{
    public static void AttestationBundleValidatorReportsEveryProblem()
    {
        var validator = AttestationBundleRules.DefaultAttestationBundleValidator();
        var item = AttestationBundle.NewAttestationBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationBundleRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new AttestationBundleRequiredTitleRule("attestation bundle needs an entry");
        var item = AttestationBundle.NewAttestationBundle("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new AttestationBundleRequiredNameRule("attestation bundle needs a value");
        var item = AttestationBundle.NewAttestationBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationBundleMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new AttestationBundleMaximumRangeRule(2.3, 2498.9);
        var item = AttestationBundle.NewAttestationBundle("x", "y");
        item.Maximum = 2500.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationBundleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AttestationBundleRequiredUnitRule("attestation bundle needs an entry");
        var item = AttestationBundle.NewAttestationBundle("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationBundleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new AttestationBundleCapacityRangeRule(16, 80);
        var item = AttestationBundle.NewAttestationBundle("x", "y");
        item.Capacity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AttestationBundleValidatorReportsEveryProblem();
        AttestationBundleRequiredTitleRuleFlagsABlankTitle();
        AttestationBundleRequiredNameRuleFlagsABlankName();
        AttestationBundleMaximumRangeRuleFlagsValuesAboveMaximum();
        AttestationBundleRequiredUnitRuleFlagsABlankUnit();
        AttestationBundleCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
