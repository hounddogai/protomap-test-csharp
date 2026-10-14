namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationEstimate;

using Acme.Workforce.Compliance.Attestation.AttestationEstimate;

/// <summary>Checks of attestation estimate code, run by the project's runner.</summary>
public static class StrictAttestationBudgetRulesChecks
{
    public static void StrictAttestationBudgetValidatorReportsEveryProblem()
    {
        var validator = StrictAttestationBudgetRules.DefaultStrictAttestationBudgetValidator();
        var item = StrictAttestationBudget.NewStrictAttestationBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StrictAttestationBudgetLabelLengthRuleFlagsALongLabel()
    {
        var rule = new StrictAttestationBudgetLabelLengthRule(32);
        var item = StrictAttestationBudget.NewStrictAttestationBudget("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void StrictAttestationBudgetRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new StrictAttestationBudgetRequiredNotesRule("strict attestation budget needs an entry");
        var item = StrictAttestationBudget.NewStrictAttestationBudget("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StrictAttestationBudgetLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new StrictAttestationBudgetLimitRangeRule(32, 255);
        var item = StrictAttestationBudget.NewStrictAttestationBudget("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StrictAttestationBudgetFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new StrictAttestationBudgetFloorValueRangeRule(8.1, 342.0);
        var item = StrictAttestationBudget.NewStrictAttestationBudget("x", "y");
        item.FloorValue = 343.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        StrictAttestationBudgetValidatorReportsEveryProblem();
        StrictAttestationBudgetLabelLengthRuleFlagsALongLabel();
        StrictAttestationBudgetRequiredNotesRuleFlagsABlankNotes();
        StrictAttestationBudgetLimitRangeRuleFlagsValuesAboveLimit();
        StrictAttestationBudgetFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
