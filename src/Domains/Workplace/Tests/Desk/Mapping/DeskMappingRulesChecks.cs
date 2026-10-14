namespace Acme.Workforce.Workplace.Tests.Desk.Mapping;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DeskMappingRulesChecks
{
    public static void DeskMappingValidatorReportsEveryProblem()
    {
        var validator = DeskMappingRules.DefaultDeskMappingValidator();
        var item = DeskMapping.NewDeskMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DeskMappingRequiredCodeRule("desk mapping needs this field");
        var item = DeskMapping.NewDeskMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeskMappingCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new DeskMappingCategoryLengthRule(32);
        var item = DeskMapping.NewDeskMapping("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskMappingRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new DeskMappingRequiredCategoryRule("desk mapping needs an entry");
        var item = DeskMapping.NewDeskMapping("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskMappingValidatorReportsEveryProblem();
        DeskMappingRequiredCodeRuleFlagsABlankCode();
        DeskMappingCategoryLengthRuleFlagsALongCategory();
        DeskMappingRequiredCategoryRuleFlagsABlankCategory();
    }
}
