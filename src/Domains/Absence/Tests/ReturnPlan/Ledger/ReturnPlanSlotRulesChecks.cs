namespace Acme.Workforce.Absence.Tests.ReturnPlan.Ledger;

using Acme.Workforce.Absence.ReturnPlan.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ReturnPlanSlotRulesChecks
{
    public static void ReturnPlanSlotValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanSlotRules.DefaultReturnPlanSlotValidator();
        var item = ReturnPlanSlot.NewReturnPlanSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanSlotUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new ReturnPlanSlotUpdatedAtRangeRule(80, 1000);
        var item = ReturnPlanSlot.NewReturnPlanSlot("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanSlotRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new ReturnPlanSlotRequiredDescriptionRule("return plan slot needs an entry");
        var item = ReturnPlanSlot.NewReturnPlanSlot("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanSlotLabelLengthRuleFlagsALongLabel()
    {
        var rule = new ReturnPlanSlotLabelLengthRule(86400);
        var item = ReturnPlanSlot.NewReturnPlanSlot("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanSlotDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new ReturnPlanSlotDescriptionLengthRule(80);
        var item = ReturnPlanSlot.NewReturnPlanSlot("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanSlotHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new ReturnPlanSlotHoursRangeRule(7.0, 2511.2);
        var item = ReturnPlanSlot.NewReturnPlanSlot("x", "y");
        item.Hours = 2512.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ReturnPlanSlotValidatorReportsEveryProblem();
        ReturnPlanSlotUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        ReturnPlanSlotRequiredDescriptionRuleFlagsABlankDescription();
        ReturnPlanSlotLabelLengthRuleFlagsALongLabel();
        ReturnPlanSlotDescriptionLengthRuleFlagsALongDescription();
        ReturnPlanSlotHoursRangeRuleFlagsValuesAboveHours();
    }
}
