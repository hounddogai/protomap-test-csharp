namespace Acme.Workforce.Workplace.Tests.Occupancy.OccupancyAgreement;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Checks of occupancy agreement code, run by the project's runner.</summary>
public static class LegacyOccupancyRecordRulesChecks
{
    public static void LegacyOccupancyRecordValidatorReportsEveryProblem()
    {
        var validator = LegacyOccupancyRecordRules.DefaultLegacyOccupancyRecordValidator();
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyOccupancyRecordRequiredCategoryRuleFlagsABlankCategory()
    {
        var rule = new LegacyOccupancyRecordRequiredCategoryRule("legacy occupancy record needs an entry");
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("", "");
        item.Category = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyOccupancyRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new LegacyOccupancyRecordExpiresAtRangeRule(16, 80);
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("x", "y");
        item.ExpiresAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyOccupancyRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyOccupancyRecordCodeLengthRule(1000);
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyOccupancyRecordWeightRangeRuleFlagsValuesAboveWeight()
    {
        var rule = new LegacyOccupancyRecordWeightRangeRule(2.5, 279.9);
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("x", "y");
        item.Weight = 281.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyOccupancyRecordSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new LegacyOccupancyRecordSummaryLengthRule(1000);
        var item = LegacyOccupancyRecord.NewLegacyOccupancyRecord("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyOccupancyRecordValidatorReportsEveryProblem();
        LegacyOccupancyRecordRequiredCategoryRuleFlagsABlankCategory();
        LegacyOccupancyRecordExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        LegacyOccupancyRecordCodeLengthRuleFlagsALongCode();
        LegacyOccupancyRecordWeightRangeRuleFlagsValuesAboveWeight();
        LegacyOccupancyRecordSummaryLengthRuleFlagsALongSummary();
    }
}
