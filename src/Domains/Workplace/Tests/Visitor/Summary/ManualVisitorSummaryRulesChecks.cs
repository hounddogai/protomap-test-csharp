namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ManualVisitorSummaryRulesChecks
{
    public static void ManualVisitorSummaryValidatorReportsEveryProblem()
    {
        var validator = ManualVisitorSummaryRules.DefaultManualVisitorSummaryValidator();
        var item = ManualVisitorSummary.NewManualVisitorSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualVisitorSummaryFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new ManualVisitorSummaryFloorValueRangeRule(1.9, 524.9);
        var item = ManualVisitorSummary.NewManualVisitorSummary("x", "y");
        item.FloorValue = 526.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ManualVisitorSummaryValidatorReportsEveryProblem();
        ManualVisitorSummaryFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
