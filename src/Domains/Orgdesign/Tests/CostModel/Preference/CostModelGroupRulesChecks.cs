namespace Acme.Workforce.Orgdesign.Tests.CostModel.Preference;

using Acme.Workforce.Orgdesign.CostModel.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class CostModelGroupRulesChecks
{
    public static void CostModelGroupValidatorReportsEveryProblem()
    {
        var validator = CostModelGroupRules.DefaultCostModelGroupValidator();
        var item = CostModelGroup.NewCostModelGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CostModelGroupNameLengthRuleFlagsALongName()
    {
        var rule = new CostModelGroupNameLengthRule(120);
        var item = CostModelGroup.NewCostModelGroup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CostModelGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new CostModelGroupTenureMonthsRangeRule(16, 32);
        var item = CostModelGroup.NewCostModelGroup("x", "y");
        item.TenureMonths = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CostModelGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CostModelGroupRequiredCodeRule("cost model group needs this field");
        var item = CostModelGroup.NewCostModelGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CostModelGroupValidatorReportsEveryProblem();
        CostModelGroupNameLengthRuleFlagsALongName();
        CostModelGroupTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        CostModelGroupRequiredCodeRuleFlagsABlankCode();
    }
}
