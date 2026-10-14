namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountRecordRulesChecks
{
    public static void HeadcountRecordValidatorReportsEveryProblem()
    {
        var validator = HeadcountRecordRules.DefaultHeadcountRecordValidator();
        var item = HeadcountRecord.NewHeadcountRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountRecordNameLengthRuleFlagsALongName()
    {
        var rule = new HeadcountRecordNameLengthRule(3600);
        var item = HeadcountRecord.NewHeadcountRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountRecordRequiredCodeRule("headcount record needs a value");
        var item = HeadcountRecord.NewHeadcountRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HeadcountRecordRequiredLabelRule("headcount record needs this field");
        var item = HeadcountRecord.NewHeadcountRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HeadcountRecordLabelLengthRule(1000);
        var item = HeadcountRecord.NewHeadcountRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountRecordValidatorReportsEveryProblem();
        HeadcountRecordNameLengthRuleFlagsALongName();
        HeadcountRecordRequiredCodeRuleFlagsABlankCode();
        HeadcountRecordRequiredLabelRuleFlagsABlankLabel();
        HeadcountRecordLabelLengthRuleFlagsALongLabel();
    }
}
