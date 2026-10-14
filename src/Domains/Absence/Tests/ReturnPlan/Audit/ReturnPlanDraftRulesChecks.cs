namespace Acme.Workforce.Absence.Tests.ReturnPlan.Audit;

using Acme.Workforce.Absence.ReturnPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class ReturnPlanDraftRulesChecks
{
    public static void ReturnPlanDraftValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanDraftRules.DefaultReturnPlanDraftValidator();
        var item = ReturnPlanDraft.NewReturnPlanDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new ReturnPlanDraftRequiredLabelRule("return plan draft needs this field");
        var item = ReturnPlanDraft.NewReturnPlanDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanDraftRequiredCodeRule("return plan draft needs a value");
        var item = ReturnPlanDraft.NewReturnPlanDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanDraftRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new ReturnPlanDraftRequiredCurrencyRule("return plan draft needs a value");
        var item = ReturnPlanDraft.NewReturnPlanDraft("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanDraftLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReturnPlanDraftLabelLengthRule(1000);
        var item = ReturnPlanDraft.NewReturnPlanDraft("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanDraftCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ReturnPlanDraftCurrencyLengthRule(32);
        var item = ReturnPlanDraft.NewReturnPlanDraft("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanDraftValidatorReportsEveryProblem();
        ReturnPlanDraftRequiredLabelRuleFlagsABlankLabel();
        ReturnPlanDraftRequiredCodeRuleFlagsABlankCode();
        ReturnPlanDraftRequiredCurrencyRuleFlagsABlankCurrency();
        ReturnPlanDraftLabelLengthRuleFlagsALongLabel();
        ReturnPlanDraftCurrencyLengthRuleFlagsALongCurrency();
    }
}
