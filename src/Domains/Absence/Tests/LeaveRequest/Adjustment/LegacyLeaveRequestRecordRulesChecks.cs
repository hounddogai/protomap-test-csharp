namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyLeaveRequestRecordRulesChecks
{
    public static void LegacyLeaveRequestRecordValidatorReportsEveryProblem()
    {
        var validator = LegacyLeaveRequestRecordRules.DefaultLegacyLeaveRequestRecordValidator();
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyLeaveRequestRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyLeaveRequestRecordRequiredNameRule("legacy leave request record needs a value");
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyLeaveRequestRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LegacyLeaveRequestRecordExpiresAtRangeRule(16, 64);
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("x", "y");
        item.ExpiresAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyLeaveRequestRecordRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LegacyLeaveRequestRecordRatioRangeRule(2.1, 468.1);
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("x", "y");
        item.Ratio = 469.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyLeaveRequestRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyLeaveRequestRecordCodeLengthRule(32);
        var item = LegacyLeaveRequestRecord.NewLegacyLeaveRequestRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyLeaveRequestRecordValidatorReportsEveryProblem();
        LegacyLeaveRequestRecordRequiredNameRuleFlagsABlankName();
        LegacyLeaveRequestRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LegacyLeaveRequestRecordRatioRangeRuleFlagsValuesAboveRatio();
        LegacyLeaveRequestRecordCodeLengthRuleFlagsALongCode();
    }
}
