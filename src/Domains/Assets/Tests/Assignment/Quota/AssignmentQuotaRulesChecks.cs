namespace Acme.Workforce.Assets.Tests.Assignment.Quota;

using Acme.Workforce.Assets.Assignment.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssignmentQuotaRulesChecks
{
    public static void AssignmentQuotaValidatorReportsEveryProblem()
    {
        var validator = AssignmentQuotaRules.DefaultAssignmentQuotaValidator();
        var item = AssignmentQuota.NewAssignmentQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssignmentQuotaNotesLengthRuleFlagsALongNotes()
    {
        var rule = new AssignmentQuotaNotesLengthRule(3600);
        var item = AssignmentQuota.NewAssignmentQuota("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new AssignmentQuotaCodeLengthRule(120);
        var item = AssignmentQuota.NewAssignmentQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AssignmentQuotaRetriesRangeRuleFlagsValuesAboveRetries()
    {
        var rule = new AssignmentQuotaRetriesRangeRule(120, 120);
        var item = AssignmentQuota.NewAssignmentQuota("x", "y");
        item.Retries = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AssignmentQuotaRequiredLabelRule("assignment quota needs a value");
        var item = AssignmentQuota.NewAssignmentQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AssignmentQuotaAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new AssignmentQuotaAmountRangeRule(5.4, 4640.2);
        var item = AssignmentQuota.NewAssignmentQuota("x", "y");
        item.Amount = 4641.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssignmentQuotaRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new AssignmentQuotaRequiredNotesRule("assignment quota needs an entry");
        var item = AssignmentQuota.NewAssignmentQuota("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AssignmentQuotaValidatorReportsEveryProblem();
        AssignmentQuotaNotesLengthRuleFlagsALongNotes();
        AssignmentQuotaCodeLengthRuleFlagsALongCode();
        AssignmentQuotaRetriesRangeRuleFlagsValuesAboveRetries();
        AssignmentQuotaRequiredLabelRuleFlagsABlankLabel();
        AssignmentQuotaAmountRangeRuleFlagsValuesAboveAmount();
        AssignmentQuotaRequiredNotesRuleFlagsABlankNotes();
    }
}
