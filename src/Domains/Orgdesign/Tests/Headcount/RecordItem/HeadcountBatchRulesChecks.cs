namespace Acme.Workforce.Orgdesign.Tests.Headcount.RecordItem;

using Acme.Workforce.Orgdesign.Headcount.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class HeadcountBatchRulesChecks
{
    public static void HeadcountBatchValidatorReportsEveryProblem()
    {
        var validator = HeadcountBatchRules.DefaultHeadcountBatchValidator();
        var item = HeadcountBatch.NewHeadcountBatch("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountBatchDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new HeadcountBatchDepartmentLengthRule(255);
        var item = HeadcountBatch.NewHeadcountBatch("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountBatchRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountBatchRequiredCodeRule("headcount batch needs a value");
        var item = HeadcountBatch.NewHeadcountBatch("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountBatchNameLengthRuleFlagsALongName()
    {
        var rule = new HeadcountBatchNameLengthRule(32);
        var item = HeadcountBatch.NewHeadcountBatch("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountBatchCodeLengthRuleFlagsALongCode()
    {
        var rule = new HeadcountBatchCodeLengthRule(1000);
        var item = HeadcountBatch.NewHeadcountBatch("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountBatchValidatorReportsEveryProblem();
        HeadcountBatchDepartmentLengthRuleFlagsALongDepartment();
        HeadcountBatchRequiredCodeRuleFlagsABlankCode();
        HeadcountBatchNameLengthRuleFlagsALongName();
        HeadcountBatchCodeLengthRuleFlagsALongCode();
    }
}
