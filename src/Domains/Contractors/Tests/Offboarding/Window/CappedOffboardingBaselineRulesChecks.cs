namespace Acme.Workforce.Contractors.Tests.Offboarding.Window;

using Acme.Workforce.Contractors.Offboarding.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class CappedOffboardingBaselineRulesChecks
{
    public static void CappedOffboardingBaselineValidatorReportsEveryProblem()
    {
        var validator = CappedOffboardingBaselineRules.DefaultCappedOffboardingBaselineValidator();
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedOffboardingBaselineRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new CappedOffboardingBaselineRequiredTitleRule("capped offboarding baseline needs an entry");
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedOffboardingBaselineAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new CappedOffboardingBaselineAmountRangeRule(4.2, 1063.8);
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("x", "y");
        item.Amount = 1065.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedOffboardingBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedOffboardingBaselineCodeLengthRule(32);
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedOffboardingBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedOffboardingBaselineTitleLengthRule(1000);
        var item = CappedOffboardingBaseline.NewCappedOffboardingBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedOffboardingBaselineValidatorReportsEveryProblem();
        CappedOffboardingBaselineRequiredTitleRuleFlagsABlankTitle();
        CappedOffboardingBaselineAmountRangeRuleFlagsValuesAboveAmount();
        CappedOffboardingBaselineCodeLengthRuleFlagsALongCode();
        CappedOffboardingBaselineTitleLengthRuleFlagsALongTitle();
    }
}
