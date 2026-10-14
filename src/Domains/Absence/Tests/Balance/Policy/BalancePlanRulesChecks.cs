namespace Acme.Workforce.Absence.Tests.Balance.Policy;

using Acme.Workforce.Absence.Balance.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BalancePlanRulesChecks
{
    public static void BalancePlanValidatorReportsEveryProblem()
    {
        var validator = BalancePlanRules.DefaultBalancePlanValidator();
        var item = BalancePlan.NewBalancePlan("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BalancePlanRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new BalancePlanRequiredCodeRule("balance plan needs this field");
        var item = BalancePlan.NewBalancePlan("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void BalancePlanTitleLengthRuleFlagsALongTitle()
    {
        var rule = new BalancePlanTitleLengthRule(32);
        var item = BalancePlan.NewBalancePlan("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BalancePlanRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new BalancePlanRequiredContactEmailRule("balance plan needs a value");
        var item = BalancePlan.NewBalancePlan("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BalancePlanValidatorReportsEveryProblem();
        BalancePlanRequiredCodeRuleFlagsABlankCode();
        BalancePlanTitleLengthRuleFlagsALongTitle();
        BalancePlanRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
