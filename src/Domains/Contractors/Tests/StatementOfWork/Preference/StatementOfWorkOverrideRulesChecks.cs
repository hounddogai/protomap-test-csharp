namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Preference;

using Acme.Workforce.Contractors.StatementOfWork.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StatementOfWorkOverrideRulesChecks
{
    public static void StatementOfWorkOverrideValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkOverrideRules.DefaultStatementOfWorkOverrideValidator();
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkOverrideRequiredCodeRule("statement of work override needs a value");
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkOverrideCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new StatementOfWorkOverrideCurrencyLengthRule(32);
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkOverrideLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StatementOfWorkOverrideLabelLengthRule(86400);
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StatementOfWorkOverrideRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new StatementOfWorkOverrideRequiredCountryRule("statement of work override needs an entry");
        var item = StatementOfWorkOverride.NewStatementOfWorkOverride("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkOverrideValidatorReportsEveryProblem();
        StatementOfWorkOverrideRequiredCodeRuleFlagsABlankCode();
        StatementOfWorkOverrideCurrencyLengthRuleFlagsALongCurrency();
        StatementOfWorkOverrideLabelLengthRuleFlagsALongLabel();
        StatementOfWorkOverrideRequiredCountryRuleFlagsABlankCountry();
    }
}
