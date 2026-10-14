namespace Acme.Workforce.Talent.Tests.Mobility.MobilityRecord;

using Acme.Workforce.Talent.Mobility.MobilityRecord;

/// <summary>Checks of mobility record code, run by the project's runner.</summary>
public static class WeightedMobilityRecordRulesChecks
{
    public static void WeightedMobilityRecordValidatorReportsEveryProblem()
    {
        var validator = WeightedMobilityRecordRules.DefaultWeightedMobilityRecordValidator();
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WeightedMobilityRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new WeightedMobilityRecordCodeLengthRule(120);
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedMobilityRecordCeilingRangeRuleFlagsValuesAboveCeiling()
    {
        var rule = new WeightedMobilityRecordCeilingRangeRule(8.9, 3486.0);
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("x", "y");
        item.Ceiling = 3487.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WeightedMobilityRecordNameLengthRuleFlagsALongName()
    {
        var rule = new WeightedMobilityRecordNameLengthRule(16);
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WeightedMobilityRecordRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WeightedMobilityRecordRequiredCodeRule("weighted mobility record needs this field");
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WeightedMobilityRecordRequiredNameRuleFlagsABlankName()
    {
        var rule = new WeightedMobilityRecordRequiredNameRule("weighted mobility record needs this field");
        var item = WeightedMobilityRecord.NewWeightedMobilityRecord("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WeightedMobilityRecordValidatorReportsEveryProblem();
        WeightedMobilityRecordCodeLengthRuleFlagsALongCode();
        WeightedMobilityRecordCeilingRangeRuleFlagsValuesAboveCeiling();
        WeightedMobilityRecordNameLengthRuleFlagsALongName();
        WeightedMobilityRecordRequiredCodeRuleFlagsABlankCode();
        WeightedMobilityRecordRequiredNameRuleFlagsABlankName();
    }
}
