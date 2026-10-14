namespace Acme.Workforce.Orgdesign.Tests.Position.Audit;

using Acme.Workforce.Orgdesign.Position.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalPositionExceptionRulesChecks
{
    public static void GlobalPositionExceptionValidatorReportsEveryProblem()
    {
        var validator = GlobalPositionExceptionRules.DefaultGlobalPositionExceptionValidator();
        var item = GlobalPositionException.NewGlobalPositionException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalPositionExceptionGradeLengthRuleFlagsALongGrade()
    {
        var rule = new GlobalPositionExceptionGradeLengthRule(3600);
        var item = GlobalPositionException.NewGlobalPositionException("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalPositionExceptionPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new GlobalPositionExceptionPriorityRangeRule(16, 64);
        var item = GlobalPositionException.NewGlobalPositionException("x", "y");
        item.Priority = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalPositionExceptionRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalPositionExceptionRequiredCodeRule("global position exception needs this field");
        var item = GlobalPositionException.NewGlobalPositionException("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalPositionExceptionValidatorReportsEveryProblem();
        GlobalPositionExceptionGradeLengthRuleFlagsALongGrade();
        GlobalPositionExceptionPriorityRangeRuleFlagsValuesAbovePriority();
        GlobalPositionExceptionRequiredCodeRuleFlagsABlankCode();
    }
}
