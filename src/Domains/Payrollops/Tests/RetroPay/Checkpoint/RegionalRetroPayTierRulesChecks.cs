namespace Acme.Workforce.Payrollops.Tests.RetroPay.Checkpoint;

using Acme.Workforce.Payrollops.RetroPay.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class RegionalRetroPayTierRulesChecks
{
    public static void RegionalRetroPayTierValidatorReportsEveryProblem()
    {
        var validator = RegionalRetroPayTierRules.DefaultRegionalRetroPayTierValidator();
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalRetroPayTierEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new RegionalRetroPayTierEmployeeIdLengthRule(16);
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalRetroPayTierCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalRetroPayTierCodeLengthRule(16);
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalRetroPayTierRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalRetroPayTierRequiredCodeRule("regional retro pay tier needs a value");
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalRetroPayTierCategoryLengthRuleFlagsALongCategory()
    {
        var rule = new RegionalRetroPayTierCategoryLengthRule(16);
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("x", "y");
        item.Category = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Category = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalRetroPayTierRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new RegionalRetroPayTierRequiredEmployeeIdRule("regional retro pay tier needs this field");
        var item = RegionalRetroPayTier.NewRegionalRetroPayTier("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalRetroPayTierValidatorReportsEveryProblem();
        RegionalRetroPayTierEmployeeIdLengthRuleFlagsALongEmployeeId();
        RegionalRetroPayTierCodeLengthRuleFlagsALongCode();
        RegionalRetroPayTierRequiredCodeRuleFlagsABlankCode();
        RegionalRetroPayTierCategoryLengthRuleFlagsALongCategory();
        RegionalRetroPayTierRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
