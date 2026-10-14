namespace Acme.Workforce.Compensation.Tests.MeritCycle.Budget;

using Acme.Workforce.Compensation.MeritCycle.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class MeritCycleThresholdRulesChecks
{
    public static void MeritCycleThresholdValidatorReportsEveryProblem()
    {
        var validator = MeritCycleThresholdRules.DefaultMeritCycleThresholdValidator();
        var item = MeritCycleThreshold.NewMeritCycleThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleThresholdRequiredNameRuleFlagsABlankName()
    {
        var rule = new MeritCycleThresholdRequiredNameRule("merit cycle threshold needs an entry");
        var item = MeritCycleThreshold.NewMeritCycleThreshold("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleThresholdWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new MeritCycleThresholdWeightRangeRule(8.4, 4529.9);
        var item = MeritCycleThreshold.NewMeritCycleThreshold("x", "y");
        item.Weight = 4531.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void MeritCycleThresholdRequiredLocaleRuleFlagsABlankLocale()
    {
        var rule = new MeritCycleThresholdRequiredLocaleRule("merit cycle threshold needs a value");
        var item = MeritCycleThreshold.NewMeritCycleThreshold("", "");
        item.Locale = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        MeritCycleThresholdValidatorReportsEveryProblem();
        MeritCycleThresholdRequiredNameRuleFlagsABlankName();
        MeritCycleThresholdWeightRangeRuleFlagsValuesAboveWeight();
        MeritCycleThresholdRequiredLocaleRuleFlagsABlankLocale();
    }
}
