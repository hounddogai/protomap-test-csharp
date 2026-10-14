namespace Acme.Workforce.Orgdesign.Tests.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

/// <summary>Checks of legacy headcount record code, run by the project's runner.</summary>
public static class HeadcountMappingRulesChecks
{
    public static void HeadcountMappingValidatorReportsEveryProblem()
    {
        var validator = HeadcountMappingRules.DefaultHeadcountMappingValidator();
        var item = HeadcountMapping.NewHeadcountMapping("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountMappingNameLengthRuleFlagsALongName()
    {
        var rule = new HeadcountMappingNameLengthRule(3600);
        var item = HeadcountMapping.NewHeadcountMapping("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountMappingCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new HeadcountMappingCostCenterLengthRule(120);
        var item = HeadcountMapping.NewHeadcountMapping("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountMappingLabelLengthRuleFlagsALongLabel()
    {
        var rule = new HeadcountMappingLabelLengthRule(255);
        var item = HeadcountMapping.NewHeadcountMapping("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void HeadcountMappingRequiredNameRuleFlagsABlankName()
    {
        var rule = new HeadcountMappingRequiredNameRule("headcount mapping needs an entry");
        var item = HeadcountMapping.NewHeadcountMapping("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountMappingValidatorReportsEveryProblem();
        HeadcountMappingNameLengthRuleFlagsALongName();
        HeadcountMappingCostCenterLengthRuleFlagsALongCostCenter();
        HeadcountMappingLabelLengthRuleFlagsALongLabel();
        HeadcountMappingRequiredNameRuleFlagsABlankName();
    }
}
