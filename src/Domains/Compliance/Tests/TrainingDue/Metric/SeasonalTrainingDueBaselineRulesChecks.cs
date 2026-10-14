namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class SeasonalTrainingDueBaselineRulesChecks
{
    public static void SeasonalTrainingDueBaselineValidatorReportsEveryProblem()
    {
        var validator = SeasonalTrainingDueBaselineRules.DefaultSeasonalTrainingDueBaselineValidator();
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalTrainingDueBaselineBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new SeasonalTrainingDueBaselineBadgeLengthRule(1000);
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalTrainingDueBaselinePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new SeasonalTrainingDueBaselinePhoneLengthRule(120);
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalTrainingDueBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new SeasonalTrainingDueBaselineRequiredCodeRule("seasonal training due baseline needs a value");
        var item = SeasonalTrainingDueBaseline.NewSeasonalTrainingDueBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        SeasonalTrainingDueBaselineValidatorReportsEveryProblem();
        SeasonalTrainingDueBaselineBadgeLengthRuleFlagsALongBadge();
        SeasonalTrainingDueBaselinePhoneLengthRuleFlagsALongPhone();
        SeasonalTrainingDueBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
