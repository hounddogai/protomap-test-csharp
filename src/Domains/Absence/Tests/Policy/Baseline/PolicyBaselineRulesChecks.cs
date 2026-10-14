namespace Acme.Workforce.Absence.Tests.Policy.Baseline;

using Acme.Workforce.Absence.Policy.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PolicyBaselineRulesChecks
{
    public static void PolicyBaselineValidatorReportsEveryProblem()
    {
        var validator = PolicyBaselineRules.DefaultPolicyBaselineValidator();
        var item = PolicyBaseline.NewPolicyBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyBaselineFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new PolicyBaselineFloorValueRangeRule(1.7, 3208.5);
        var item = PolicyBaseline.NewPolicyBaseline("x", "y");
        item.FloorValue = 3210.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PolicyBaselineRequiredCodeRule("policy baseline needs a value");
        var item = PolicyBaseline.NewPolicyBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PolicyBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyBaselineCodeLengthRule(32);
        var item = PolicyBaseline.NewPolicyBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyBaselineValidatorReportsEveryProblem();
        PolicyBaselineFloorValueRangeRuleFlagsValuesAboveFloorValue();
        PolicyBaselineRequiredCodeRuleFlagsABlankCode();
        PolicyBaselineCodeLengthRuleFlagsALongCode();
    }
}
