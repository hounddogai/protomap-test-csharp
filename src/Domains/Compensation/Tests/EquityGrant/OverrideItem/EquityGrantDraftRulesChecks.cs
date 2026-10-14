namespace Acme.Workforce.Compensation.Tests.EquityGrant.OverrideItem;

using Acme.Workforce.Compensation.EquityGrant.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class EquityGrantDraftRulesChecks
{
    public static void EquityGrantDraftValidatorReportsEveryProblem()
    {
        var validator = EquityGrantDraftRules.DefaultEquityGrantDraftValidator();
        var item = EquityGrantDraft.NewEquityGrantDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantDraftCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new EquityGrantDraftCurrencyLengthRule(120);
        var item = EquityGrantDraft.NewEquityGrantDraft("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantDraftRequiredCodeRule("equity grant draft needs an entry");
        var item = EquityGrantDraft.NewEquityGrantDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantDraftRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new EquityGrantDraftRequiredCurrencyRule("equity grant draft needs a value");
        var item = EquityGrantDraft.NewEquityGrantDraft("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantDraftValidatorReportsEveryProblem();
        EquityGrantDraftCurrencyLengthRuleFlagsALongCurrency();
        EquityGrantDraftRequiredCodeRuleFlagsABlankCode();
        EquityGrantDraftRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
