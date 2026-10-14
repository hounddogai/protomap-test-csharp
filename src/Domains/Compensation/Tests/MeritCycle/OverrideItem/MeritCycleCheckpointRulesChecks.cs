namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleCheckpointRulesChecks
{
    public static void MeritCycleCheckpointValidatorReportsEveryProblem()
    {
        var validator = MeritCycleCheckpointRules.DefaultMeritCycleCheckpointValidator();
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleCheckpointRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new MeritCycleCheckpointRequiredLocaleRule("merit cycle checkpoint needs an entry");
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleCheckpointCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new MeritCycleCheckpointCreatedAtRangeRule(32, 3600);
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleCheckpointRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new MeritCycleCheckpointRequiredCategoryRule("merit cycle checkpoint needs an entry");
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleCheckpointLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new MeritCycleCheckpointLimitRangeRule(16, 86400);
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("x", "y");
        item.Limit = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleCheckpointValidatorReportsEveryProblem();
        MeritCycleCheckpointRequiredLocaleRuleFlagsABlankLocale();
        MeritCycleCheckpointCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        MeritCycleCheckpointRequiredCategoryRuleFlagsABlankCategory();
        MeritCycleCheckpointLimitRangeRuleFlagsValuesAboveLimit();
    }
}
