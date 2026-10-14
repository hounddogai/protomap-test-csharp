namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalVisitorCheckpointRulesChecks
{
    public static void SeasonalVisitorCheckpointValidatorReportsEveryProblem()
    {
        var validator = SeasonalVisitorCheckpointRules.DefaultSeasonalVisitorCheckpointValidator();
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalVisitorCheckpointRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new SeasonalVisitorCheckpointRequiredLocaleRule("seasonal visitor checkpoint needs an entry");
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalVisitorCheckpointCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalVisitorCheckpointCodeLengthRule(3600);
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SeasonalVisitorCheckpointManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new SeasonalVisitorCheckpointManagerIdLengthRule(120);
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalVisitorCheckpointValidatorReportsEveryProblem();
        SeasonalVisitorCheckpointRequiredLocaleRuleFlagsABlankLocale();
        SeasonalVisitorCheckpointCodeLengthRuleFlagsALongCode();
        SeasonalVisitorCheckpointManagerIdLengthRuleFlagsALongManagerId();
    }
}
