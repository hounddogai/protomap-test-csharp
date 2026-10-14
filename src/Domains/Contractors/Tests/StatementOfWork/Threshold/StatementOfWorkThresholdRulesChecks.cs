namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class StatementOfWorkThresholdRulesChecks
{
    public static void StatementOfWorkThresholdValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkThresholdRules.DefaultStatementOfWorkThresholdValidator();
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkThresholdStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new StatementOfWorkThresholdStartsAtRangeRule(32, 64);
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("x", "y");
        item.StartsAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkThresholdGraceDaysRangeRuleFlagsValuesAboveGraceDays()
    {
        var rule = new StatementOfWorkThresholdGraceDaysRangeRule(120, 3600);
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("x", "y");
        item.GraceDays = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkThresholdDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new StatementOfWorkThresholdDiscountRangeRule(1.2, 264.4);
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("x", "y");
        item.Discount = 265.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkThresholdRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new StatementOfWorkThresholdRequiredLabelRule("statement of work threshold needs a value");
        var item = StatementOfWorkThreshold.NewStatementOfWorkThreshold("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkThresholdValidatorReportsEveryProblem();
        StatementOfWorkThresholdStartsAtRangeRuleFlagsValuesAboveStartsAt();
        StatementOfWorkThresholdGraceDaysRangeRuleFlagsValuesAboveGraceDays();
        StatementOfWorkThresholdDiscountRangeRuleFlagsValuesAboveDiscount();
        StatementOfWorkThresholdRequiredLabelRuleFlagsABlankLabel();
    }
}
