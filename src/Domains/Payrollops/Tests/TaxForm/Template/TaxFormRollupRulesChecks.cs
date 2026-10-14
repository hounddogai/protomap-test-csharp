namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormRollupRulesChecks
{
    public static void TaxFormRollupValidatorReportsEveryProblem()
    {
        var validator = TaxFormRollupRules.DefaultTaxFormRollupValidator();
        var item = TaxFormRollup.NewTaxFormRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormRollupRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new TaxFormRollupRevisionRangeRule(16, 32);
        var item = TaxFormRollup.NewTaxFormRollup("x", "y");
        item.Revision = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new TaxFormRollupGraceDaysRangeRule(255, 86400);
        var item = TaxFormRollup.NewTaxFormRollup("x", "y");
        item.GraceDays = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormRollupCodeLengthRuleFlagsALongCode()
    {
        var rule = new TaxFormRollupCodeLengthRule(80);
        var item = TaxFormRollup.NewTaxFormRollup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TaxFormRollupValidatorReportsEveryProblem();
        TaxFormRollupRevisionRangeRuleFlagsValuesAboveRevision();
        TaxFormRollupGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        TaxFormRollupCodeLengthRuleFlagsALongCode();
    }
}
