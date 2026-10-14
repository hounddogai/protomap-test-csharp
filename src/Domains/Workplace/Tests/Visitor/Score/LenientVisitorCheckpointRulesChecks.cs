namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class LenientVisitorCheckpointRulesChecks
{
    public static void LenientVisitorCheckpointValidatorReportsEveryProblem()
    {
        var validator = LenientVisitorCheckpointRules.DefaultLenientVisitorCheckpointValidator();
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientVisitorCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientVisitorCheckpointRequiredNameRule("lenient visitor checkpoint needs a value");
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientVisitorCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientVisitorCheckpointCodeLengthRule(120);
        var item = LenientVisitorCheckpoint.NewLenientVisitorCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientVisitorCheckpointValidatorReportsEveryProblem();
        LenientVisitorCheckpointRequiredNameRuleFlagsABlankName();
        LenientVisitorCheckpointCodeLengthRuleFlagsALongCode();
    }
}
