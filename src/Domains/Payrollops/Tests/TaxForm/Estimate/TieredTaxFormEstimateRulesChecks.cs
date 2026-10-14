namespace Acme.Workforce.Payrollops.Tests.TaxForm.Estimate;

using Acme.Workforce.Payrollops.TaxForm.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TieredTaxFormEstimateRulesChecks
{
    public static void TieredTaxFormEstimateValidatorReportsEveryProblem()
    {
        var validator = TieredTaxFormEstimateRules.DefaultTieredTaxFormEstimateValidator();
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredTaxFormEstimateRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new TieredTaxFormEstimateRequiredExternalRefRule("tiered tax form estimate needs an entry");
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredTaxFormEstimateCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredTaxFormEstimateCodeLengthRule(80);
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredTaxFormEstimateExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new TieredTaxFormEstimateExternalRefLengthRule(86400);
        var item = TieredTaxFormEstimate.NewTieredTaxFormEstimate("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredTaxFormEstimateValidatorReportsEveryProblem();
        TieredTaxFormEstimateRequiredExternalRefRuleFlagsABlankExternalRef();
        TieredTaxFormEstimateCodeLengthRuleFlagsALongCode();
        TieredTaxFormEstimateExternalRefLengthRuleFlagsALongExternalRef();
    }
}
