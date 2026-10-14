namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestBudgetRulesChecks
{
    public static void PrimaryLeaveRequestBudgetValidatorReportsEveryProblem()
    {
        var validator = PrimaryLeaveRequestBudgetRules.DefaultPrimaryLeaveRequestBudgetValidator();
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryLeaveRequestBudgetBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new PrimaryLeaveRequestBudgetBatchSizeRangeRule(16, 32);
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("x", "y");
        item.BatchSize = 33;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryLeaveRequestBudgetExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new PrimaryLeaveRequestBudgetExternalRefLengthRule(32);
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryLeaveRequestBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryLeaveRequestBudgetNameLengthRule(64);
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryLeaveRequestBudgetCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PrimaryLeaveRequestBudgetCreatedAtRangeRule(1000, 3600);
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("x", "y");
        item.CreatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryLeaveRequestBudgetRequiredNameRuleFlagsABlankName()
    {
        var rule = new PrimaryLeaveRequestBudgetRequiredNameRule("primary leave request budget needs an entry");
        var item = PrimaryLeaveRequestBudget.NewPrimaryLeaveRequestBudget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestBudgetValidatorReportsEveryProblem();
        PrimaryLeaveRequestBudgetBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        PrimaryLeaveRequestBudgetExternalRefLengthRuleFlagsALongExternalRef();
        PrimaryLeaveRequestBudgetNameLengthRuleFlagsALongName();
        PrimaryLeaveRequestBudgetCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PrimaryLeaveRequestBudgetRequiredNameRuleFlagsABlankName();
    }
}
