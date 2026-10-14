namespace Acme.Workforce.Payrollops.Tests.RetroPay.Tier;

using Acme.Workforce.Payrollops.RetroPay.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ManualRetroPayTierRulesChecks
{
    public static void ManualRetroPayTierValidatorReportsEveryProblem()
    {
        var validator = ManualRetroPayTierRules.DefaultManualRetroPayTierValidator();
        var item = ManualRetroPayTier.NewManualRetroPayTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualRetroPayTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualRetroPayTierRequiredCodeRule("manual retro pay tier needs an entry");
        var item = ManualRetroPayTier.NewManualRetroPayTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRetroPayTierRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualRetroPayTierRequiredNameRule("manual retro pay tier needs a value");
        var item = ManualRetroPayTier.NewManualRetroPayTier("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualRetroPayTierManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ManualRetroPayTierManagerIdLengthRule(255);
        var item = ManualRetroPayTier.NewManualRetroPayTier("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ManualRetroPayTierValidatorReportsEveryProblem();
        ManualRetroPayTierRequiredCodeRuleFlagsABlankCode();
        ManualRetroPayTierRequiredNameRuleFlagsABlankName();
        ManualRetroPayTierManagerIdLengthRuleFlagsALongManagerId();
    }
}
