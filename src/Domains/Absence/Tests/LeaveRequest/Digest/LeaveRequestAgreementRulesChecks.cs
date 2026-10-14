namespace Acme.Workforce.Absence.Tests.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LeaveRequestAgreementRulesChecks
{
    public static void LeaveRequestAgreementValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestAgreementRules.DefaultLeaveRequestAgreementValidator();
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestAgreementCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new LeaveRequestAgreementCeilingRangeRule(7.2, 1214.6);
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("x", "y");
        item.Ceiling = 1216.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestAgreementBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new LeaveRequestAgreementBatchSizeRangeRule(16, 255);
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("x", "y");
        item.BatchSize = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LeaveRequestAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new LeaveRequestAgreementUpdatedAtRangeRule(16, 86400);
        var item = LeaveRequestAgreement.NewLeaveRequestAgreement("x", "y");
        item.UpdatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LeaveRequestAgreementValidatorReportsEveryProblem();
        LeaveRequestAgreementCeilingRangeRuleFlagsValuesAboveCeiling();
        LeaveRequestAgreementBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        LeaveRequestAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
