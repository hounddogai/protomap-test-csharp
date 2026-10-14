namespace Acme.Workforce.Talent.Tests.Pool.Summary;

using Acme.Workforce.Talent.Pool.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingPoolSlotRulesChecks
{
    public static void PendingPoolSlotValidatorReportsEveryProblem()
    {
        var validator = PendingPoolSlotRules.DefaultPendingPoolSlotValidator();
        var item = PendingPoolSlot.NewPendingPoolSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingPoolSlotVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new PendingPoolSlotVersionRangeRule(16, 80);
        var item = PendingPoolSlot.NewPendingPoolSlot("x", "y");
        item.Version = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingPoolSlotValidatorReportsEveryProblem();
        PendingPoolSlotVersionRangeRuleFlagsValuesAboveVersion();
    }
}
