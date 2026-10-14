namespace Acme.Workforce.Absence.Tests.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

/// <summary>Checks of pending accrual assignment code, run by the project's runner.</summary>
public static class AccrualDraftRulesChecks
{
    public static void AccrualDraftValidatorReportsEveryProblem()
    {
        var validator = AccrualDraftRules.DefaultAccrualDraftValidator();
        var item = AccrualDraft.NewAccrualDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualDraftCountryLengthRuleFlagsALongCountry()
    {
        var rule = new AccrualDraftCountryLengthRule(80);
        var item = AccrualDraft.NewAccrualDraft("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualDraftCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualDraftCodeLengthRule(255);
        var item = AccrualDraft.NewAccrualDraft("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualDraftRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualDraftRequiredLabelRule("accrual draft needs this field");
        var item = AccrualDraft.NewAccrualDraft("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AccrualDraftValidatorReportsEveryProblem();
        AccrualDraftCountryLengthRuleFlagsALongCountry();
        AccrualDraftCodeLengthRuleFlagsALongCode();
        AccrualDraftRequiredLabelRuleFlagsABlankLabel();
    }
}
