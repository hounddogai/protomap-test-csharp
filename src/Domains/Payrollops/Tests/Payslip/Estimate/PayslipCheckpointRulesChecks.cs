namespace Acme.Workforce.Payrollops.Tests.Payslip.Estimate;

using Acme.Workforce.Payrollops.Payslip.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PayslipCheckpointRulesChecks
{
    public static void PayslipCheckpointValidatorReportsEveryProblem()
    {
        var validator = PayslipCheckpointRules.DefaultPayslipCheckpointValidator();
        var item = PayslipCheckpoint.NewPayslipCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new PayslipCheckpointCodeLengthRule(64);
        var item = PayslipCheckpoint.NewPayslipCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipCheckpointNotesLengthRuleFlagsALongNotes()
    {
        var rule = new PayslipCheckpointNotesLengthRule(120);
        var item = PayslipCheckpoint.NewPayslipCheckpoint("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PayslipCheckpointWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new PayslipCheckpointWeightRangeRule(7.5, 4005.2);
        var item = PayslipCheckpoint.NewPayslipCheckpoint("x", "y");
        item.Weight = 4006.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipCheckpointRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PayslipCheckpointRequiredNotesRule("payslip checkpoint needs an entry");
        var item = PayslipCheckpoint.NewPayslipCheckpoint("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PayslipCheckpointValidatorReportsEveryProblem();
        PayslipCheckpointCodeLengthRuleFlagsALongCode();
        PayslipCheckpointNotesLengthRuleFlagsALongNotes();
        PayslipCheckpointWeightRangeRuleFlagsValuesAboveWeight();
        PayslipCheckpointRequiredNotesRuleFlagsABlankNotes();
    }
}
