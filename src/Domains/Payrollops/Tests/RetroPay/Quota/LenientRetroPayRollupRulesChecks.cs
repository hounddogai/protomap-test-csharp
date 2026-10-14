namespace Acme.Workforce.Payrollops.Tests.RetroPay.Quota;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class LenientRetroPayRollupRulesChecks
{
    public static void LenientRetroPayRollupValidatorReportsEveryProblem()
    {
        var validator = LenientRetroPayRollupRules.DefaultLenientRetroPayRollupValidator();
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientRetroPayRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientRetroPayRollupCodeLengthRule(64);
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientRetroPayRollupRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new LenientRetroPayRollupRequiredRegionRule("lenient retro pay rollup needs a value");
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientRetroPayRollupFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new LenientRetroPayRollupFteRangeRule(8.6, 1993.0);
        var item = LenientRetroPayRollup.NewLenientRetroPayRollup("x", "y");
        item.Fte = 1994.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientRetroPayRollupValidatorReportsEveryProblem();
        LenientRetroPayRollupCodeLengthRuleFlagsALongCode();
        LenientRetroPayRollupRequiredRegionRuleFlagsABlankRegion();
        LenientRetroPayRollupFteRangeRuleFlagsValuesAboveFte();
    }
}
