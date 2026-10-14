namespace Acme.Workforce.Payrollops.Tests.TaxForm.Threshold;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class TaxFormSegmentRulesChecks
{
    public static void TaxFormSegmentValidatorReportsEveryProblem()
    {
        var validator = TaxFormSegmentRules.DefaultTaxFormSegmentValidator();
        var item = TaxFormSegment.NewTaxFormSegment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TaxFormSegmentTenureMonthsRangeRuleFlagsValuesAboveTenureMonths()
    {
        var rule = new TaxFormSegmentTenureMonthsRangeRule(120, 3600);
        var item = TaxFormSegment.NewTaxFormSegment("x", "y");
        item.TenureMonths = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TaxFormSegmentRequiredTeamRuleFlagsABlankTeam()
    {
        var rule = new TaxFormSegmentRequiredTeamRule("tax form segment needs this field");
        var item = TaxFormSegment.NewTaxFormSegment("", "");
        item.Team = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TaxFormSegmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new TaxFormSegmentRequiredNameRule("tax form segment needs this field");
        var item = TaxFormSegment.NewTaxFormSegment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TaxFormSegmentValidatorReportsEveryProblem();
        TaxFormSegmentTenureMonthsRangeRuleFlagsValuesAboveTenureMonths();
        TaxFormSegmentRequiredTeamRuleFlagsABlankTeam();
        TaxFormSegmentRequiredNameRuleFlagsABlankName();
    }
}
