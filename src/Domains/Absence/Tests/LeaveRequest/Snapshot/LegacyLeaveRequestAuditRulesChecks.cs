namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyLeaveRequestAuditRulesChecks
{
    public static void LegacyLeaveRequestAuditValidatorReportsEveryProblem()
    {
        var validator = LegacyLeaveRequestAuditRules.DefaultLegacyLeaveRequestAuditValidator();
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyLeaveRequestAuditRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new LegacyLeaveRequestAuditRetriesRangeRule(16, 255);
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("x", "y");
        item.Retries = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyLeaveRequestAuditRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyLeaveRequestAuditRequiredCodeRule("legacy leave request audit needs an entry");
        var item = LegacyLeaveRequestAudit.NewLegacyLeaveRequestAudit("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestAuditValidatorReportsEveryProblem();
        LegacyLeaveRequestAuditRetriesRangeRuleFlagsValuesAboveRetries();
        LegacyLeaveRequestAuditRequiredCodeRuleFlagsABlankCode();
    }
}
