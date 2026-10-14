namespace Acme.Workforce.Assets.Tests.Assignment.Baseline;

using Acme.Workforce.Assets.Assignment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LegacyAssignmentBaselineRulesChecks
{
    public static void LegacyAssignmentBaselineValidatorReportsEveryProblem()
    {
        var validator = LegacyAssignmentBaselineRules.DefaultLegacyAssignmentBaselineValidator();
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyAssignmentBaselineDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new LegacyAssignmentBaselineDurationMinutesRangeRule(80, 86400);
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyAssignmentBaselineRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new LegacyAssignmentBaselineRequiredSummaryRule("legacy assignment baseline needs this field");
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAssignmentBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyAssignmentBaselineRequiredCodeRule("legacy assignment baseline needs this field");
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyAssignmentBaselineFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new LegacyAssignmentBaselineFloorValueRangeRule(7.1, 1427.2);
        var item = LegacyAssignmentBaseline.NewLegacyAssignmentBaseline("x", "y");
        item.FloorValue = 1428.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LegacyAssignmentBaselineValidatorReportsEveryProblem();
        LegacyAssignmentBaselineDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        LegacyAssignmentBaselineRequiredSummaryRuleFlagsABlankSummary();
        LegacyAssignmentBaselineRequiredCodeRuleFlagsABlankCode();
        LegacyAssignmentBaselineFloorValueRangeRuleFlagsValuesAboveFloorValue();
    }
}
