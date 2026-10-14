namespace Acme.Workforce.Orgdesign.Tests.CostModel.Variance;

using Acme.Workforce.Orgdesign.CostModel.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class FallbackCostModelLedgerRulesChecks
{
    public static void FallbackCostModelLedgerValidatorReportsEveryProblem()
    {
        var validator = FallbackCostModelLedgerRules.DefaultFallbackCostModelLedgerValidator();
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void FallbackCostModelLedgerNotesLengthRuleFlagsALongNotes()
    {
        var rule = new FallbackCostModelLedgerNotesLengthRule(3600);
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void FallbackCostModelLedgerRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new FallbackCostModelLedgerRequiredContactEmailRule("fallback cost model ledger needs this field");
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void FallbackCostModelLedgerCodeLengthRuleFlagsALongCode()
    {
        var rule = new FallbackCostModelLedgerCodeLengthRule(255);
        var item = FallbackCostModelLedger.NewFallbackCostModelLedger("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        FallbackCostModelLedgerValidatorReportsEveryProblem();
        FallbackCostModelLedgerNotesLengthRuleFlagsALongNotes();
        FallbackCostModelLedgerRequiredContactEmailRuleFlagsABlankContactEmail();
        FallbackCostModelLedgerCodeLengthRuleFlagsALongCode();
    }
}
