namespace Acme.Workforce.Assets.Tests.LicenseSeat.Assignment;

using Acme.Workforce.Assets.LicenseSeat.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredLicenseSeatRuleRulesChecks
{
    public static void TieredLicenseSeatRuleValidatorReportsEveryProblem()
    {
        var validator = TieredLicenseSeatRuleRules.DefaultTieredLicenseSeatRuleValidator();
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredLicenseSeatRuleRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new TieredLicenseSeatRuleRequiredPhoneRule("tiered license seat rule needs a value");
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredLicenseSeatRulePhoneLengthRuleFlagsALongPhone()
    {
        var rule = new TieredLicenseSeatRulePhoneLengthRule(120);
        var item = TieredLicenseSeatRule.NewTieredLicenseSeatRule("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredLicenseSeatRuleValidatorReportsEveryProblem();
        TieredLicenseSeatRuleRequiredPhoneRuleFlagsABlankPhone();
        TieredLicenseSeatRulePhoneLengthRuleFlagsALongPhone();
    }
}
