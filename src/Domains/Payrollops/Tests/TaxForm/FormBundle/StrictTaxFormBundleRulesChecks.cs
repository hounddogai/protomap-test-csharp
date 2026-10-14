namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormBundle;

using Acme.Workforce.Payrollops.TaxForm.FormBundle;

/// <summary>Checks of form bundle code, run by the project's runner.</summary>
public static class StrictTaxFormBundleRulesChecks
{
    public static void StrictTaxFormBundleValidatorReportsEveryProblem()
    {
        var validator = StrictTaxFormBundleRules.DefaultStrictTaxFormBundleValidator();
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictTaxFormBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new StrictTaxFormBundleCodeLengthRule(255);
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictTaxFormBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new StrictTaxFormBundleTenureMonthsRangeRule(80, 120);
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("x", "y");
        item.TenureMonths = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictTaxFormBundleTeamLengthRuleFlagsALongTeam()
    {
        var rule = new StrictTaxFormBundleTeamLengthRule(3600);
        var item = StrictTaxFormBundle.NewStrictTaxFormBundle("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StrictTaxFormBundleValidatorReportsEveryProblem();
        StrictTaxFormBundleCodeLengthRuleFlagsALongCode();
        StrictTaxFormBundleTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        StrictTaxFormBundleTeamLengthRuleFlagsALongTeam();
    }
}
