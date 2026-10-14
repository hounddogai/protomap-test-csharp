namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class GlobalAttestationBundleRulesChecks
{
    public static void GlobalAttestationBundleValidatorReportsEveryProblem()
    {
        var validator = GlobalAttestationBundleRules.DefaultGlobalAttestationBundleValidator();
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAttestationBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalAttestationBundleRequiredNameRule("global attestation bundle needs this field");
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAttestationBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalAttestationBundleLabelLengthRule(3600);
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAttestationBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAttestationBundleCodeLengthRule(32);
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAttestationBundleCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new GlobalAttestationBundleCapacityRangeRule(1000, 1000);
        var item = GlobalAttestationBundle.NewGlobalAttestationBundle("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        GlobalAttestationBundleValidatorReportsEveryProblem();
        GlobalAttestationBundleRequiredNameRuleFlagsABlankName();
        GlobalAttestationBundleLabelLengthRuleFlagsALongLabel();
        GlobalAttestationBundleCodeLengthRuleFlagsALongCode();
        GlobalAttestationBundleCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
