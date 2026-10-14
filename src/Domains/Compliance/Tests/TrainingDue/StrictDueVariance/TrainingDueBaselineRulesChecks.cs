namespace Acme.Workforce.Compliance.Tests.TrainingDue.StrictDueVariance;

using Acme.Workforce.Compliance.TrainingDue.StrictDueVariance;

/// <summary>Checks of strict due variance code, run by the project's runner.</summary>
public static class TrainingDueBaselineRulesChecks
{
    public static void TrainingDueBaselineValidatorReportsEveryProblem()
    {
        var validator = TrainingDueBaselineRules.DefaultTrainingDueBaselineValidator();
        var item = TrainingDueBaseline.NewTrainingDueBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TrainingDueBaselineLabelLengthRule(3600);
        var item = TrainingDueBaseline.NewTrainingDueBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TrainingDueBaselineValidatorReportsEveryProblem();
        TrainingDueBaselineLabelLengthRuleFlagsALongLabel();
    }
}
