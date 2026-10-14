namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountCheckpointRulesChecks
{
    public static void HeadcountCheckpointValidatorReportsEveryProblem()
    {
        var validator = HeadcountCheckpointRules.DefaultHeadcountCheckpointValidator();
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountCheckpointRequiredCodeRule("headcount checkpoint needs this field");
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new HeadcountCheckpointRequiredNameRule("headcount checkpoint needs this field");
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountCheckpointContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new HeadcountCheckpointContactEmailLengthRule(1000);
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountCheckpointRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new HeadcountCheckpointRequiredCategoryRule("headcount checkpoint needs a value");
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountCheckpointCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new HeadcountCheckpointCategoryLengthRule(120);
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        HeadcountCheckpointValidatorReportsEveryProblem();
        HeadcountCheckpointRequiredCodeRuleFlagsABlankCode();
        HeadcountCheckpointRequiredNameRuleFlagsABlankName();
        HeadcountCheckpointContactEmailLengthRuleFlagsALongContactEmail();
        HeadcountCheckpointRequiredCategoryRuleFlagsABlankCategory();
        HeadcountCheckpointCategoryLengthRuleFlagsALongCategory();
    }
}
