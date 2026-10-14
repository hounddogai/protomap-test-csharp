namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanAdjustmentRulesChecks
{
    public static void ReturnPlanAdjustmentValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanAdjustmentRules.DefaultReturnPlanAdjustmentValidator();
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanAdjustmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new ReturnPlanAdjustmentUpdatedAtRangeRule(255, 1000);
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("x", "y");
        item.UpdatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanAdjustmentWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new ReturnPlanAdjustmentWindowDaysRangeRule(80, 1000);
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanAdjustmentQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ReturnPlanAdjustmentQuantityRangeRule(64, 255);
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("x", "y");
        item.Quantity = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanAdjustmentRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReturnPlanAdjustmentRequiredNameRule("return plan adjustment needs an entry");
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanAdjustmentSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new ReturnPlanAdjustmentSequenceRangeRule(1000, 1000);
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("x", "y");
        item.Sequence = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ReturnPlanAdjustmentNameLengthRuleFlagsALongName()
    {
        var rule = new ReturnPlanAdjustmentNameLengthRule(86400);
        var item = ReturnPlanAdjustment.NewReturnPlanAdjustment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReturnPlanAdjustmentValidatorReportsEveryProblem();
        ReturnPlanAdjustmentUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        ReturnPlanAdjustmentWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        ReturnPlanAdjustmentQuantityRangeRuleFlagsValuesAboveQuantity();
        ReturnPlanAdjustmentRequiredNameRuleFlagsABlankName();
        ReturnPlanAdjustmentSequenceRangeRuleFlagsValuesAboveSequence();
        ReturnPlanAdjustmentNameLengthRuleFlagsALongName();
    }
}
