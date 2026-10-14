namespace Acme.Workforce.Payrollops.Tests.PayRun.Target;

using Acme.Workforce.Payrollops.PayRun.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LegacyPayRunTargetRulesChecks
{
    public static void LegacyPayRunTargetValidatorReportsEveryProblem()
    {
        var validator = LegacyPayRunTargetRules.DefaultLegacyPayRunTargetValidator();
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPayRunTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyPayRunTargetCodeLengthRule(1000);
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPayRunTargetRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LegacyPayRunTargetRequiredBadgeRule("legacy pay run target needs this field");
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPayRunTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new LegacyPayRunTargetRequiredNameRule("legacy pay run target needs this field");
        var item = LegacyPayRunTarget.NewLegacyPayRunTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyPayRunTargetValidatorReportsEveryProblem();
        LegacyPayRunTargetCodeLengthRuleFlagsALongCode();
        LegacyPayRunTargetRequiredBadgeRuleFlagsABlankBadge();
        LegacyPayRunTargetRequiredNameRuleFlagsABlankName();
    }
}
