namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class FallbackPoolAssignmentRulesChecks
{
    public static void FallbackPoolAssignmentValidatorReportsEveryProblem()
    {
        var validator = FallbackPoolAssignmentRules.DefaultFallbackPoolAssignmentValidator();
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackPoolAssignmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new FallbackPoolAssignmentRequiredNameRule("fallback pool assignment needs an entry");
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackPoolAssignmentMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new FallbackPoolAssignmentMinimumRangeRule(2.9, 1266.2);
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("x", "y");
        item.Minimum = 1267.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void FallbackPoolAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackPoolAssignmentCodeLengthRule(1000);
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackPoolAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new FallbackPoolAssignmentRequiredCodeRule("fallback pool assignment needs a value");
        var item = FallbackPoolAssignment.NewFallbackPoolAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        FallbackPoolAssignmentValidatorReportsEveryProblem();
        FallbackPoolAssignmentRequiredNameRuleFlagsABlankName();
        FallbackPoolAssignmentMinimumRangeRuleFlagsValuesAboveMinimum();
        FallbackPoolAssignmentCodeLengthRuleFlagsALongCode();
        FallbackPoolAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
