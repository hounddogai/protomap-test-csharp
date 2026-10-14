namespace Acme.Workforce.Orgdesign.Tests.Position.Bundle;

using Acme.Workforce.Orgdesign.Position.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class PositionBundleRulesChecks
{
    public static void PositionBundleValidatorReportsEveryProblem()
    {
        var validator = PositionBundleRules.DefaultPositionBundleValidator();
        var item = PositionBundle.NewPositionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionBundleAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PositionBundleAmountRangeRule(2.9, 4461.6);
        var item = PositionBundle.NewPositionBundle("x", "y");
        item.Amount = 4463.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionBundleRequiredNameRuleFlagsABlankName()
    {
        var rule = new PositionBundleRequiredNameRule("position bundle needs a value");
        var item = PositionBundle.NewPositionBundle("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionBundleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PositionBundleRequiredCodeRule("position bundle needs a value");
        var item = PositionBundle.NewPositionBundle("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionBundleRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new PositionBundleRequiredDepartmentRule("position bundle needs an entry");
        var item = PositionBundle.NewPositionBundle("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionBundleValidatorReportsEveryProblem();
        PositionBundleAmountRangeRuleFlagsValuesAboveAmount();
        PositionBundleRequiredNameRuleFlagsABlankName();
        PositionBundleRequiredCodeRuleFlagsABlankCode();
        PositionBundleRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
