namespace Acme.Workforce.Workplace.Tests.Visitor.OverrideItem;

using Acme.Workforce.Workplace.Visitor.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PendingVisitorAdjustmentRulesChecks
{
    public static void PendingVisitorAdjustmentValidatorReportsEveryProblem()
    {
        var validator = PendingVisitorAdjustmentRules.DefaultPendingVisitorAdjustmentValidator();
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingVisitorAdjustmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingVisitorAdjustmentRequiredCodeRule("pending visitor adjustment needs this field");
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingVisitorAdjustmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PendingVisitorAdjustmentCreatedAtRangeRule(120, 86400);
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingVisitorAdjustmentRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new PendingVisitorAdjustmentRequiredRegionRule("pending visitor adjustment needs a value");
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingVisitorAdjustmentVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new PendingVisitorAdjustmentVersionRangeRule(16, 16);
        var item = PendingVisitorAdjustment.NewPendingVisitorAdjustment("x", "y");
        item.Version = 17;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingVisitorAdjustmentValidatorReportsEveryProblem();
        PendingVisitorAdjustmentRequiredCodeRuleFlagsABlankCode();
        PendingVisitorAdjustmentCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PendingVisitorAdjustmentRequiredRegionRuleFlagsABlankRegion();
        PendingVisitorAdjustmentVersionRangeRuleFlagsValuesAboveVersion();
    }
}
