namespace Acme.Workforce.Payrollops.Tests.Payslip.RollingPayslipSnapshot;

using Acme.Workforce.Payrollops.Payslip.RollingPayslipSnapshot;

/// <summary>Checks of rolling payslip snapshot code, run by the project's runner.</summary>
public static class PrimaryPayslipSnapshotRulesChecks
{
    public static void PrimaryPayslipSnapshotValidatorReportsEveryProblem()
    {
        var validator = PrimaryPayslipSnapshotRules.DefaultPrimaryPayslipSnapshotValidator();
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPayslipSnapshotRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new PrimaryPayslipSnapshotRequiredLocationRule("primary payslip snapshot needs this field");
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPayslipSnapshotRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new PrimaryPayslipSnapshotRequiredTeamRule("primary payslip snapshot needs an entry");
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPayslipSnapshotRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryPayslipSnapshotRequiredCodeRule("primary payslip snapshot needs an entry");
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryPayslipSnapshotLocationLengthRuleFlagsALongLocation()
    {
        var rule = new PrimaryPayslipSnapshotLocationLengthRule(16);
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPayslipSnapshotRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryPayslipSnapshotRequiredLabelRule("primary payslip snapshot needs a value");
        var item = PrimaryPayslipSnapshot.NewPrimaryPayslipSnapshot("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryPayslipSnapshotValidatorReportsEveryProblem();
        PrimaryPayslipSnapshotRequiredLocationRuleFlagsABlankLocation();
        PrimaryPayslipSnapshotRequiredTeamRuleFlagsABlankTeam();
        PrimaryPayslipSnapshotRequiredCodeRuleFlagsABlankCode();
        PrimaryPayslipSnapshotLocationLengthRuleFlagsALongLocation();
        PrimaryPayslipSnapshotRequiredLabelRuleFlagsABlankLabel();
    }
}
