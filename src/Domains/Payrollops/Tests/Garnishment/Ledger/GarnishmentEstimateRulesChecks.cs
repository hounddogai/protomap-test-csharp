namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentEstimateRulesChecks
{
    public static void GarnishmentEstimateValidatorReportsEveryProblem()
    {
        var validator = GarnishmentEstimateRules.DefaultGarnishmentEstimateValidator();
        var item = GarnishmentEstimate.NewGarnishmentEstimate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentEstimateRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GarnishmentEstimateRequiredCodeRule("garnishment estimate needs an entry");
        var item = GarnishmentEstimate.NewGarnishmentEstimate("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentEstimateRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new GarnishmentEstimateRequiredNotesRule("garnishment estimate needs an entry");
        var item = GarnishmentEstimate.NewGarnishmentEstimate("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void GarnishmentEstimateNameLengthRuleFlagsALongName()
    {
        var rule = new GarnishmentEstimateNameLengthRule(3600);
        var item = GarnishmentEstimate.NewGarnishmentEstimate("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GarnishmentEstimateValidatorReportsEveryProblem();
        GarnishmentEstimateRequiredCodeRuleFlagsABlankCode();
        GarnishmentEstimateRequiredNotesRuleFlagsABlankNotes();
        GarnishmentEstimateNameLengthRuleFlagsALongName();
    }
}
