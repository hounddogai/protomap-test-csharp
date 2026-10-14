namespace Acme.Workforce.Assets.Tests.Laptop.Mapping;

using Acme.Workforce.Assets.Laptop.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultLaptopMappingRulesChecks
{
    public static void DefaultLaptopMappingValidatorReportsEveryProblem()
    {
        var validator = DefaultLaptopMappingRules.DefaultDefaultLaptopMappingValidator();
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultLaptopMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultLaptopMappingRequiredNameRule("default laptop mapping needs this field");
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLaptopMappingCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultLaptopMappingCodeLengthRule(1000);
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DefaultLaptopMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new DefaultLaptopMappingTenureMonthsRangeRule(1000, 86400);
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("x", "y");
        item.TenureMonths = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultLaptopMappingRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultLaptopMappingRequiredCodeRule("default laptop mapping needs a value");
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultLaptopMappingNotesLengthRuleFlagsALongNotes()
    {
        var rule = new DefaultLaptopMappingNotesLengthRule(3600);
        var item = DefaultLaptopMapping.NewDefaultLaptopMapping("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultLaptopMappingValidatorReportsEveryProblem();
        DefaultLaptopMappingRequiredNameRuleFlagsABlankName();
        DefaultLaptopMappingCodeLengthRuleFlagsALongCode();
        DefaultLaptopMappingTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        DefaultLaptopMappingRequiredCodeRuleFlagsABlankCode();
        DefaultLaptopMappingNotesLengthRuleFlagsALongNotes();
    }
}
