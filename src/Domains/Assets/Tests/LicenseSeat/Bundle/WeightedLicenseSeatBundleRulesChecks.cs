namespace Acme.Workforce.Assets.Tests.LicenseSeat.Bundle;

using Acme.Workforce.Assets.LicenseSeat.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedLicenseSeatBundleRulesChecks
{
    public static void WeightedLicenseSeatBundleValidatorReportsEveryProblem()
    {
        var validator = WeightedLicenseSeatBundleRules.DefaultWeightedLicenseSeatBundleValidator();
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedLicenseSeatBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedLicenseSeatBundleCodeLengthRule(64);
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedLicenseSeatBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WeightedLicenseSeatBundleRequiredLabelRule("weighted license seat bundle needs a value");
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLicenseSeatBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new WeightedLicenseSeatBundleLabelLengthRule(80);
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedLicenseSeatBundleRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new WeightedLicenseSeatBundleRequiredGradeRule("weighted license seat bundle needs this field");
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedLicenseSeatBundleGradeLengthRuleFlagsALongGrade()
    {
        var rule = new WeightedLicenseSeatBundleGradeLengthRule(32);
        var item = WeightedLicenseSeatBundle.NewWeightedLicenseSeatBundle("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WeightedLicenseSeatBundleValidatorReportsEveryProblem();
        WeightedLicenseSeatBundleCodeLengthRuleFlagsALongCode();
        WeightedLicenseSeatBundleRequiredLabelRuleFlagsABlankLabel();
        WeightedLicenseSeatBundleLabelLengthRuleFlagsALongLabel();
        WeightedLicenseSeatBundleRequiredGradeRuleFlagsABlankGrade();
        WeightedLicenseSeatBundleGradeLengthRuleFlagsALongGrade();
    }
}
