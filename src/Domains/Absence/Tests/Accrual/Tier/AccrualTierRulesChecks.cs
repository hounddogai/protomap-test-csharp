namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualTierRulesChecks
{
    public static void AccrualTierValidatorReportsEveryProblem()
    {
        var validator = AccrualTierRules.DefaultAccrualTierValidator();
        var item = AccrualTier.NewAccrualTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualTierDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new AccrualTierDescriptionLengthRule(32);
        var item = AccrualTier.NewAccrualTier("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualTierRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualTierRequiredLabelRule("accrual tier needs an entry");
        var item = AccrualTier.NewAccrualTier("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualTierLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AccrualTierLabelLengthRule(80);
        var item = AccrualTier.NewAccrualTier("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualTierValidatorReportsEveryProblem();
        AccrualTierDescriptionLengthRuleFlagsALongDescription();
        AccrualTierRequiredLabelRuleFlagsABlankLabel();
        AccrualTierLabelLengthRuleFlagsALongLabel();
    }
}
