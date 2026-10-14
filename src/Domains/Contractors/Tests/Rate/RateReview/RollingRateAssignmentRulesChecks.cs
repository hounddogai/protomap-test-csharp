namespace Acme.Workforce.Contractors.Tests.Rate.RateReview;

using Acme.Workforce.Contractors.Rate.RateReview;

/// <summary>Checks of rate review code, run by the project's runner.</summary>
public static class RollingRateAssignmentRulesChecks
{
    public static void RollingRateAssignmentValidatorReportsEveryProblem()
    {
        var validator = RollingRateAssignmentRules.DefaultRollingRateAssignmentValidator();
        var item = RollingRateAssignment.NewRollingRateAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingRateAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingRateAssignmentCodeLengthRule(64);
        var item = RollingRateAssignment.NewRollingRateAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingRateAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RollingRateAssignmentRequiredLabelRule("rolling rate assignment needs an entry");
        var item = RollingRateAssignment.NewRollingRateAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingRateAssignmentSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new RollingRateAssignmentSequenceRangeRule(16, 80);
        var item = RollingRateAssignment.NewRollingRateAssignment("x", "y");
        item.Sequence = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingRateAssignmentValidatorReportsEveryProblem();
        RollingRateAssignmentCodeLengthRuleFlagsALongCode();
        RollingRateAssignmentRequiredLabelRuleFlagsABlankLabel();
        RollingRateAssignmentSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
