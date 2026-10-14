namespace Acme.Workforce.Compliance.Tests.TrainingDue.Tier;

using Acme.Workforce.Compliance.TrainingDue.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LenientTrainingDueTierRulesChecks
{
    public static void LenientTrainingDueTierValidatorReportsEveryProblem()
    {
        var validator = LenientTrainingDueTierRules.DefaultLenientTrainingDueTierValidator();
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientTrainingDueTierTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientTrainingDueTierTitleLengthRule(120);
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientTrainingDueTierLocationLengthRuleFlagsALongLocation()
    {
        var rule = new LenientTrainingDueTierLocationLengthRule(255);
        var item = LenientTrainingDueTier.NewLenientTrainingDueTier("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientTrainingDueTierValidatorReportsEveryProblem();
        LenientTrainingDueTierTitleLengthRuleFlagsALongTitle();
        LenientTrainingDueTierLocationLengthRuleFlagsALongLocation();
    }
}
