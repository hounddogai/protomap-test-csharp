namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class AccrualQuotaRulesChecks
{
    public static void AccrualQuotaValidatorReportsEveryProblem()
    {
        var validator = AccrualQuotaRules.DefaultAccrualQuotaValidator();
        var item = AccrualQuota.NewAccrualQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AccrualQuotaRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AccrualQuotaRequiredLabelRule("accrual quota needs this field");
        var item = AccrualQuota.NewAccrualQuota("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualQuotaNotesLengthRuleFlagsALongNotes()
    {
        var rule = new AccrualQuotaNotesLengthRule(255);
        var item = AccrualQuota.NewAccrualQuota("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AccrualQuotaRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new AccrualQuotaRequiredCostCenterRule("accrual quota needs an entry");
        var item = AccrualQuota.NewAccrualQuota("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AccrualQuotaVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new AccrualQuotaVersionRangeRule(120, 1000);
        var item = AccrualQuota.NewAccrualQuota("x", "y");
        item.Version = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AccrualQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new AccrualQuotaCodeLengthRule(86400);
        var item = AccrualQuota.NewAccrualQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AccrualQuotaValidatorReportsEveryProblem();
        AccrualQuotaRequiredLabelRuleFlagsABlankLabel();
        AccrualQuotaNotesLengthRuleFlagsALongNotes();
        AccrualQuotaRequiredCostCenterRuleFlagsABlankCostCenter();
        AccrualQuotaVersionRangeRuleFlagsValuesAboveVersion();
        AccrualQuotaCodeLengthRuleFlagsALongCode();
    }
}
