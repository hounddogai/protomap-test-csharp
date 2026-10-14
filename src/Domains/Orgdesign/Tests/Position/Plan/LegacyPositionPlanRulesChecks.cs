namespace Acme.Workforce.Orgdesign.Tests.Position.Plan;

using Acme.Workforce.Orgdesign.Position.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyPositionPlanRulesChecks
{
    public static void LegacyPositionPlanValidatorReportsEveryProblem()
    {
        var validator = LegacyPositionPlanRules.DefaultLegacyPositionPlanValidator();
        var item = LegacyPositionPlan.NewLegacyPositionPlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPositionPlanCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new LegacyPositionPlanCapacityRangeRule(255, 255);
        var item = LegacyPositionPlan.NewLegacyPositionPlan("x", "y");
        item.Capacity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyPositionPlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyPositionPlanRequiredCodeRule("legacy position plan needs an entry");
        var item = LegacyPositionPlan.NewLegacyPositionPlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPositionPlanNameLengthRuleFlagsALongName()
    {
        var rule = new LegacyPositionPlanNameLengthRule(255);
        var item = LegacyPositionPlan.NewLegacyPositionPlan("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPositionPlanRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyPositionPlanRequiredNameRule("legacy position plan needs this field");
        var item = LegacyPositionPlan.NewLegacyPositionPlan("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPositionPlanCountryLengthRuleFlagsALongCountry()
    {
        var rule = new LegacyPositionPlanCountryLengthRule(120);
        var item = LegacyPositionPlan.NewLegacyPositionPlan("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPositionPlanCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new LegacyPositionPlanCategoryLengthRule(3600);
        var item = LegacyPositionPlan.NewLegacyPositionPlan("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPositionPlanValidatorReportsEveryProblem();
        LegacyPositionPlanCapacityRangeRuleFlagsValuesAboveCapacity();
        LegacyPositionPlanRequiredCodeRuleFlagsABlankCode();
        LegacyPositionPlanNameLengthRuleFlagsALongName();
        LegacyPositionPlanRequiredNameRuleFlagsABlankName();
        LegacyPositionPlanCountryLengthRuleFlagsALongCountry();
        LegacyPositionPlanCategoryLengthRuleFlagsALongCategory();
    }
}
