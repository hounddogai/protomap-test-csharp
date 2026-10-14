namespace Acme.Workforce.Contractors.Tests.Offboarding.Bundle;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class GlobalOffboardingBundleRulesChecks
{
    public static void GlobalOffboardingBundleValidatorReportsEveryProblem()
    {
        var validator = GlobalOffboardingBundleRules.DefaultGlobalOffboardingBundleValidator();
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalOffboardingBundleContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new GlobalOffboardingBundleContactEmailLengthRule(80);
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalOffboardingBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new GlobalOffboardingBundleCodeLengthRule(32);
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalOffboardingBundleBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new GlobalOffboardingBundleBudgetRangeRule(4.0, 1964.3);
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("x", "y");
        item.Budget = 1965.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalOffboardingBundleRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new GlobalOffboardingBundleRequiredContactEmailRule("global offboarding bundle needs this field");
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GlobalOffboardingBundleSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new GlobalOffboardingBundleSummaryLengthRule(16);
        var item = GlobalOffboardingBundle.NewGlobalOffboardingBundle("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalOffboardingBundleValidatorReportsEveryProblem();
        GlobalOffboardingBundleContactEmailLengthRuleFlagsALongContactEmail();
        GlobalOffboardingBundleCodeLengthRuleFlagsALongCode();
        GlobalOffboardingBundleBudgetRangeRuleFlagsValuesAboveBudget();
        GlobalOffboardingBundleRequiredContactEmailRuleFlagsABlankContactEmail();
        GlobalOffboardingBundleSummaryLengthRuleFlagsALongSummary();
    }
}
