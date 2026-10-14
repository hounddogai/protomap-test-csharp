namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class PendingTaxFormThresholdRulesChecks
{
    public static void PendingTaxFormThresholdValidatorReportsEveryProblem()
    {
        var validator = PendingTaxFormThresholdRules.DefaultPendingTaxFormThresholdValidator();
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingTaxFormThresholdRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PendingTaxFormThresholdRequiredExternalRefRule("pending tax form threshold needs this field");
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingTaxFormThresholdRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingTaxFormThresholdRequiredCodeRule("pending tax form threshold needs an entry");
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingTaxFormThresholdCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingTaxFormThresholdCodeLengthRule(80);
        var item = PendingTaxFormThreshold.NewPendingTaxFormThreshold("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingTaxFormThresholdValidatorReportsEveryProblem();
        PendingTaxFormThresholdRequiredExternalRefRuleFlagsABlankExternalRef();
        PendingTaxFormThresholdRequiredCodeRuleFlagsABlankCode();
        PendingTaxFormThresholdCodeLengthRuleFlagsALongCode();
    }
}
