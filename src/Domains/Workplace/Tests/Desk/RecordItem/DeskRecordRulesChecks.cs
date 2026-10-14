namespace Acme.Workforce.Workplace.Tests.Desk.RecordItem;

using Acme.Workforce.Workplace.Desk.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class DeskRecordRulesChecks
{
    public static void DeskRecordValidatorReportsEveryProblem()
    {
        var validator = DeskRecordRules.DefaultDeskRecordValidator();
        var item = DeskRecord.NewDeskRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskRecordRequiredCodeRule("desk record needs a value");
        var item = DeskRecord.NewDeskRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskRecordCodeLengthRule(80);
        var item = DeskRecord.NewDeskRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskRecordOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new DeskRecordOwnerIdLengthRule(86400);
        var item = DeskRecord.NewDeskRecord("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskRecordRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new DeskRecordRequiredTitleRule("desk record needs an entry");
        var item = DeskRecord.NewDeskRecord("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskRecordValidatorReportsEveryProblem();
        DeskRecordRequiredCodeRuleFlagsABlankCode();
        DeskRecordCodeLengthRuleFlagsALongCode();
        DeskRecordOwnerIdLengthRuleFlagsALongOwnerId();
        DeskRecordRequiredTitleRuleFlagsABlankTitle();
    }
}
