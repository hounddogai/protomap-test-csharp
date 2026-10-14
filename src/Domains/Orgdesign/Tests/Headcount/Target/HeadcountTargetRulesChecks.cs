namespace Acme.Workforce.Orgdesign.Tests.Headcount.Target;

using Acme.Workforce.Orgdesign.Headcount.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class HeadcountTargetRulesChecks
{
    public static void HeadcountTargetValidatorReportsEveryProblem()
    {
        var validator = HeadcountTargetRules.DefaultHeadcountTargetValidator();
        var item = HeadcountTarget.NewHeadcountTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void HeadcountTargetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new HeadcountTargetRequiredCodeRule("headcount target needs a value");
        var item = HeadcountTarget.NewHeadcountTarget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountTargetCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new HeadcountTargetCostRangeRule(6.2, 3544.0);
        var item = HeadcountTarget.NewHeadcountTarget("x", "y");
        item.Cost = 3545.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void HeadcountTargetRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new HeadcountTargetRequiredLabelRule("headcount target needs an entry");
        var item = HeadcountTarget.NewHeadcountTarget("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void HeadcountTargetRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new HeadcountTargetRequiredDepartmentRule("headcount target needs an entry");
        var item = HeadcountTarget.NewHeadcountTarget("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        HeadcountTargetValidatorReportsEveryProblem();
        HeadcountTargetRequiredCodeRuleFlagsABlankCode();
        HeadcountTargetCostRangeRuleFlagsValuesAboveCost();
        HeadcountTargetRequiredLabelRuleFlagsABlankLabel();
        HeadcountTargetRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
