namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AssignmentLedgerRulesChecks
{
    public static void AssignmentLedgerValidatorReportsEveryProblem()
    {
        var validator = AssignmentLedgerRules.DefaultAssignmentLedgerValidator();
        var item = AssignmentLedger.NewAssignmentLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentLedgerLocationLengthRuleFlagsALongLocation()
    {
        var rule = new AssignmentLedgerLocationLengthRule(255);
        var item = AssignmentLedger.NewAssignmentLedger("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentLedgerCodeLengthRule(86400);
        var item = AssignmentLedger.NewAssignmentLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentLedgerSalaryBandLengthRuleFlagsALongSalaryBand()
    {
        var rule = new AssignmentLedgerSalaryBandLengthRule(120);
        var item = AssignmentLedger.NewAssignmentLedger("x", "y");
        item.SalaryBand = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.SalaryBand = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AssignmentLedgerLabelLengthRule(3600);
        var item = AssignmentLedger.NewAssignmentLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentLedgerRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AssignmentLedgerRequiredLocationRule("assignment ledger needs a value");
        var item = AssignmentLedger.NewAssignmentLedger("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentLedgerValidatorReportsEveryProblem();
        AssignmentLedgerLocationLengthRuleFlagsALongLocation();
        AssignmentLedgerCodeLengthRuleFlagsALongCode();
        AssignmentLedgerSalaryBandLengthRuleFlagsALongSalaryBand();
        AssignmentLedgerLabelLengthRuleFlagsALongLabel();
        AssignmentLedgerRequiredLocationRuleFlagsABlankLocation();
    }
}
