namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class CareerPathTargetRulesChecks
{
    public static void CareerPathTargetValidatorReportsEveryProblem()
    {
        var validator = CareerPathTargetRules.DefaultCareerPathTargetValidator();
        var item = CareerPathTarget.NewCareerPathTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathTargetExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new CareerPathTargetExternalRefLengthRule(32);
        var item = CareerPathTarget.NewCareerPathTarget("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathTargetRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new CareerPathTargetRatioRangeRule(5.0, 4116.0);
        var item = CareerPathTarget.NewCareerPathTarget("x", "y");
        item.Ratio = 4117.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new CareerPathTargetCodeLengthRule(86400);
        var item = CareerPathTarget.NewCareerPathTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CareerPathTargetValidatorReportsEveryProblem();
        CareerPathTargetExternalRefLengthRuleFlagsALongExternalRef();
        CareerPathTargetRatioRangeRuleFlagsValuesAboveRatio();
        CareerPathTargetCodeLengthRuleFlagsALongCode();
    }
}
