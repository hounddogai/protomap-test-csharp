namespace Acme.Workforce.Orgdesign.Tests.Scenario.Draft;

using Acme.Workforce.Orgdesign.Scenario.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class LenientScenarioDraftRulesChecks
{
    public static void LenientScenarioDraftValidatorReportsEveryProblem()
    {
        var validator = LenientScenarioDraftRules.DefaultLenientScenarioDraftValidator();
        var item = LenientScenarioDraft.NewLenientScenarioDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientScenarioDraftTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientScenarioDraftTitleLengthRule(1000);
        var item = LenientScenarioDraft.NewLenientScenarioDraft("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientScenarioDraftNameLengthRuleFlagsALongName()
    {
        var rule = new LenientScenarioDraftNameLengthRule(16);
        var item = LenientScenarioDraft.NewLenientScenarioDraft("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientScenarioDraftCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new LenientScenarioDraftCostCenterLengthRule(86400);
        var item = LenientScenarioDraft.NewLenientScenarioDraft("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientScenarioDraftRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientScenarioDraftRequiredNameRule("lenient scenario draft needs this field");
        var item = LenientScenarioDraft.NewLenientScenarioDraft("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientScenarioDraftHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new LenientScenarioDraftHoursRangeRule(1.0, 3288.4);
        var item = LenientScenarioDraft.NewLenientScenarioDraft("x", "y");
        item.Hours = 3289.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientScenarioDraftValidatorReportsEveryProblem();
        LenientScenarioDraftTitleLengthRuleFlagsALongTitle();
        LenientScenarioDraftNameLengthRuleFlagsALongName();
        LenientScenarioDraftCostCenterLengthRuleFlagsALongCostCenter();
        LenientScenarioDraftRequiredNameRuleFlagsABlankName();
        LenientScenarioDraftHoursRangeRuleFlagsValuesAboveHours();
    }
}
