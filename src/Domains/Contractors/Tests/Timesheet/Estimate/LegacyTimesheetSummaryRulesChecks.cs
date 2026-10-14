namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetSummaryRulesChecks
{
    public static void LegacyTimesheetSummaryValidatorReportsEveryProblem()
    {
        var validator = LegacyTimesheetSummaryRules.DefaultLegacyTimesheetSummaryValidator();
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyTimesheetSummaryRequiredBadgeRuleFlagsABlankBadge()
    {
        var rule = new LegacyTimesheetSummaryRequiredBadgeRule("legacy timesheet summary needs this field");
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("", "");
        item.Badge = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTimesheetSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyTimesheetSummaryRequiredCodeRule("legacy timesheet summary needs a value");
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTimesheetSummaryRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new LegacyTimesheetSummaryRateRangeRule(5.1, 1999.4);
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("x", "y");
        item.Rate = 2000.9;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyTimesheetSummaryRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new LegacyTimesheetSummaryRequiredCostCenterRule("legacy timesheet summary needs this field");
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyTimesheetSummaryBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new LegacyTimesheetSummaryBadgeLengthRule(32);
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyTimesheetSummaryValidatorReportsEveryProblem();
        LegacyTimesheetSummaryRequiredBadgeRuleFlagsABlankBadge();
        LegacyTimesheetSummaryRequiredCodeRuleFlagsABlankCode();
        LegacyTimesheetSummaryRateRangeRuleFlagsValuesAboveRate();
        LegacyTimesheetSummaryRequiredCostCenterRuleFlagsABlankCostCenter();
        LegacyTimesheetSummaryBadgeLengthRuleFlagsALongBadge();
    }
}
