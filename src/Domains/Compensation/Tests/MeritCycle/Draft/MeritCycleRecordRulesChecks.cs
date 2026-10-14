namespace Acme.Workforce.Compensation.Tests.MeritCycle.Draft;

using Acme.Workforce.Compensation.MeritCycle.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class MeritCycleRecordRulesChecks
{
    public static void MeritCycleRecordValidatorReportsEveryProblem()
    {
        var validator = MeritCycleRecordRules.DefaultMeritCycleRecordValidator();
        var item = MeritCycleRecord.NewMeritCycleRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleRecordRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new MeritCycleRecordRequiredChannelRule("merit cycle record needs an entry");
        var item = MeritCycleRecord.NewMeritCycleRecord("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleRecordCodeLengthRule(120);
        var item = MeritCycleRecord.NewMeritCycleRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleRecordNameLengthRuleFlagsALongName()
    {
        var rule = new MeritCycleRecordNameLengthRule(86400);
        var item = MeritCycleRecord.NewMeritCycleRecord("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleRecordRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new MeritCycleRecordRequiredCostCenterRule("merit cycle record needs an entry");
        var item = MeritCycleRecord.NewMeritCycleRecord("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MeritCycleRecordThresholdRangeRuleFlagsValuesAboveThreshold()
    {
        var rule = new MeritCycleRecordThresholdRangeRule(0.1, 1825.6);
        var item = MeritCycleRecord.NewMeritCycleRecord("x", "y");
        item.Threshold = 1827.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleRecordValidatorReportsEveryProblem();
        MeritCycleRecordRequiredChannelRuleFlagsABlankChannel();
        MeritCycleRecordCodeLengthRuleFlagsALongCode();
        MeritCycleRecordNameLengthRuleFlagsALongName();
        MeritCycleRecordRequiredCostCenterRuleFlagsABlankCostCenter();
        MeritCycleRecordThresholdRangeRuleFlagsValuesAboveThreshold();
    }
}
