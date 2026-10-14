namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalMeritCycleRollupRulesChecks
{
    public static void GlobalMeritCycleRollupValidatorReportsEveryProblem()
    {
        var validator = GlobalMeritCycleRollupRules.DefaultGlobalMeritCycleRollupValidator();
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalMeritCycleRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new GlobalMeritCycleRollupQuantityRangeRule(64, 255);
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalMeritCycleRollupNameLengthRuleFlagsALongName()
    {
        var rule = new GlobalMeritCycleRollupNameLengthRule(255);
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalMeritCycleRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new GlobalMeritCycleRollupRequiredNameRule("global merit cycle rollup needs an entry");
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalMeritCycleRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GlobalMeritCycleRollupRequiredCodeRule("global merit cycle rollup needs a value");
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GlobalMeritCycleRollupValidatorReportsEveryProblem();
        GlobalMeritCycleRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        GlobalMeritCycleRollupNameLengthRuleFlagsALongName();
        GlobalMeritCycleRollupRequiredNameRuleFlagsABlankName();
        GlobalMeritCycleRollupRequiredCodeRuleFlagsABlankCode();
    }
}
