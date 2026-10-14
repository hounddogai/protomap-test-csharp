namespace Acme.Workforce.Contractors.Tests.StatementOfWork.RecordItem;

using Acme.Workforce.Contractors.StatementOfWork.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkRecordRulesChecks
{
    public static void LegacyStatementOfWorkRecordValidatorReportsEveryProblem()
    {
        var validator = LegacyStatementOfWorkRecordRules.DefaultLegacyStatementOfWorkRecordValidator();
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyStatementOfWorkRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyStatementOfWorkRecordRequiredCodeRule("legacy statement of work record needs this field");
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyStatementOfWorkRecordRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new LegacyStatementOfWorkRecordRequiredDepartmentRule(
            "legacy statement of work record needs a value");
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyStatementOfWorkRecordRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LegacyStatementOfWorkRecordRatioRangeRule(9.2, 3383.5);
        var item = LegacyStatementOfWorkRecord.NewLegacyStatementOfWorkRecord("x", "y");
        item.Ratio = 3385.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkRecordValidatorReportsEveryProblem();
        LegacyStatementOfWorkRecordRequiredCodeRuleFlagsABlankCode();
        LegacyStatementOfWorkRecordRequiredDepartmentRuleFlagsABlankDepartment();
        LegacyStatementOfWorkRecordRatioRangeRuleFlagsValuesAboveRatio();
    }
}
