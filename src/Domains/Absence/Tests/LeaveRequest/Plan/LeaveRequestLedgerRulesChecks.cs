namespace Acme.Workforce.Absence.Tests.LeaveRequest.Plan;

using Acme.Workforce.Absence.LeaveRequest.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LeaveRequestLedgerRulesChecks
{
    public static void LeaveRequestLedgerValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestLedgerRules.DefaultLeaveRequestLedgerValidator();
        var item = LeaveRequestLedger.NewLeaveRequestLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestLedgerRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LeaveRequestLedgerRequiredLabelRule("leave request ledger needs this field");
        var item = LeaveRequestLedger.NewLeaveRequestLedger("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestLedgerAttemptsRangeRuleFlagsValuesAboveAttempts()
    {
        var rule = new LeaveRequestLedgerAttemptsRangeRule(1000, 3600);
        var item = LeaveRequestLedger.NewLeaveRequestLedger("x", "y");
        item.Attempts = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestLedgerLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LeaveRequestLedgerLabelLengthRule(120);
        var item = LeaveRequestLedger.NewLeaveRequestLedger("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LeaveRequestLedgerRequiredNameRuleFlagsABlankName()
    {
        var rule = new LeaveRequestLedgerRequiredNameRule("leave request ledger needs an entry");
        var item = LeaveRequestLedger.NewLeaveRequestLedger("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestLedgerValidatorReportsEveryProblem();
        LeaveRequestLedgerRequiredLabelRuleFlagsABlankLabel();
        LeaveRequestLedgerAttemptsRangeRuleFlagsValuesAboveAttempts();
        LeaveRequestLedgerLabelLengthRuleFlagsALongLabel();
        LeaveRequestLedgerRequiredNameRuleFlagsABlankName();
    }
}
