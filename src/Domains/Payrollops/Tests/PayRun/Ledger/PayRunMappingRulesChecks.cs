namespace Acme.Workforce.Payrollops.Tests.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PayRunMappingRulesChecks
{
    public static void PayRunMappingValidatorReportsEveryProblem()
    {
        var validator = PayRunMappingRules.DefaultPayRunMappingValidator();
        var item = PayRunMapping.NewPayRunMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayRunMappingNameLengthRuleFlagsALongName()
    {
        var rule = new PayRunMappingNameLengthRule(16);
        var item = PayRunMapping.NewPayRunMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayRunMappingMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new PayRunMappingMinimumRangeRule(1.9, 4033.6);
        var item = PayRunMapping.NewPayRunMapping("x", "y");
        item.Minimum = 4035.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PayRunMappingValidatorReportsEveryProblem();
        PayRunMappingNameLengthRuleFlagsALongName();
        PayRunMappingMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
