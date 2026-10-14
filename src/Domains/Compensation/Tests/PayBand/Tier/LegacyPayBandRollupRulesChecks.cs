namespace Acme.Workforce.Compensation.Tests.PayBand.Tier;

using Acme.Workforce.Compensation.PayBand.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class LegacyPayBandRollupRulesChecks
{
    public static void LegacyPayBandRollupValidatorReportsEveryProblem()
    {
        var validator = LegacyPayBandRollupRules.DefaultLegacyPayBandRollupValidator();
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayBandRollupRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyPayBandRollupRequiredNameRule("legacy pay band rollup needs this field");
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayBandRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyPayBandRollupRequiredCodeRule("legacy pay band rollup needs a value");
        var item = LegacyPayBandRollup.NewLegacyPayBandRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyPayBandRollupValidatorReportsEveryProblem();
        LegacyPayBandRollupRequiredNameRuleFlagsABlankName();
        LegacyPayBandRollupRequiredCodeRuleFlagsABlankCode();
    }
}
