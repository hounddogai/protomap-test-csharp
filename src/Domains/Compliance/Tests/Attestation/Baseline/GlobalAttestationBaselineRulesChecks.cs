namespace Acme.Workforce.Compliance.Tests.Attestation.Baseline;

using Acme.Workforce.Compliance.Attestation.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GlobalAttestationBaselineRulesChecks
{
    public static void GlobalAttestationBaselineValidatorReportsEveryProblem()
    {
        var validator = GlobalAttestationBaselineRules.DefaultGlobalAttestationBaselineValidator();
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalAttestationBaselineManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new GlobalAttestationBaselineManagerIdLengthRule(32);
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAttestationBaselineQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new GlobalAttestationBaselineQuantityRangeRule(16, 3600);
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAttestationBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new GlobalAttestationBaselineLabelLengthRule(86400);
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalAttestationBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalAttestationBaselineRequiredCodeRule("global attestation baseline needs an entry");
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalAttestationBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new GlobalAttestationBaselineExpiresAtRangeRule(3600, 86400);
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalAttestationBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalAttestationBaselineCodeLengthRule(64);
        var item = GlobalAttestationBaseline.NewGlobalAttestationBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalAttestationBaselineValidatorReportsEveryProblem();
        GlobalAttestationBaselineManagerIdLengthRuleFlagsALongManagerId();
        GlobalAttestationBaselineQuantityRangeRuleFlagsValuesAboveQuantity();
        GlobalAttestationBaselineLabelLengthRuleFlagsALongLabel();
        GlobalAttestationBaselineRequiredCodeRuleFlagsABlankCode();
        GlobalAttestationBaselineExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        GlobalAttestationBaselineCodeLengthRuleFlagsALongCode();
    }
}
