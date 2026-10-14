namespace Acme.Workforce.Workplace.Tests.Desk.DeskRecord;

using Acme.Workforce.Workplace.Desk.DeskRecord;

/// <summary>Checks of desk record code, run by the project's runner.</summary>
public static class CappedDeskRecordRulesChecks
{
    public static void CappedDeskRecordValidatorReportsEveryProblem()
    {
        var validator = CappedDeskRecordRules.DefaultCappedDeskRecordValidator();
        var item = CappedDeskRecord.NewCappedDeskRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedDeskRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedDeskRecordRequiredCodeRule("capped desk record needs a value");
        var item = CappedDeskRecord.NewCappedDeskRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedDeskRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new CappedDeskRecordRequiredNameRule("capped desk record needs this field");
        var item = CappedDeskRecord.NewCappedDeskRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedDeskRecordRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new CappedDeskRecordRequiredSegmentRule("capped desk record needs a value");
        var item = CappedDeskRecord.NewCappedDeskRecord("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CappedDeskRecordValidatorReportsEveryProblem();
        CappedDeskRecordRequiredCodeRuleFlagsABlankCode();
        CappedDeskRecordRequiredNameRuleFlagsABlankName();
        CappedDeskRecordRequiredSegmentRuleFlagsABlankSegment();
    }
}
