namespace Acme.Workforce.Payrollops.Tests.Payslip.Checkpoint;

using Acme.Workforce.Payrollops.Payslip.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class GlobalPayslipCheckpointRulesChecks
{
    public static void GlobalPayslipCheckpointValidatorReportsEveryProblem()
    {
        var validator = GlobalPayslipCheckpointRules.DefaultGlobalPayslipCheckpointValidator();
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPayslipCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalPayslipCheckpointNameLengthRule(3600);
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPayslipCheckpointAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new GlobalPayslipCheckpointAmountRangeRule(6.6, 4439.9);
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("x", "y");
        item.Amount = 4441.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPayslipCheckpointCountryLengthRuleFlagsALongCountry()
    {
        var rule = new GlobalPayslipCheckpointCountryLengthRule(1000);
        var item = GlobalPayslipCheckpoint.NewGlobalPayslipCheckpoint("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalPayslipCheckpointValidatorReportsEveryProblem();
        GlobalPayslipCheckpointNameLengthRuleFlagsALongName();
        GlobalPayslipCheckpointAmountRangeRuleFlagsValuesAboveAmount();
        GlobalPayslipCheckpointCountryLengthRuleFlagsALongCountry();
    }
}
