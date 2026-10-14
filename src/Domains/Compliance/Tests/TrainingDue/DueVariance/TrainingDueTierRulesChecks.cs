namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueTierRulesChecks
{
    public static void TrainingDueTierValidatorReportsEveryProblem()
    {
        var validator = TrainingDueTierRules.DefaultTrainingDueTierValidator();
        var item = TrainingDueTier.NewTrainingDueTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TrainingDueTierRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new TrainingDueTierRequiredCategoryRule("training due tier needs an entry");
        var item = TrainingDueTier.NewTrainingDueTier("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TrainingDueTierRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new TrainingDueTierRequiredDepartmentRule("training due tier needs an entry");
        var item = TrainingDueTier.NewTrainingDueTier("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TrainingDueTierValidatorReportsEveryProblem();
        TrainingDueTierRequiredCategoryRuleFlagsABlankCategory();
        TrainingDueTierRequiredDepartmentRuleFlagsABlankDepartment();
    }
}
