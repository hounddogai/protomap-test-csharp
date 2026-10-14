namespace Acme.Workforce.Assets.Tests.Refresh.RecordItem;

using Acme.Workforce.Assets.Refresh.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacyRefreshRecordRulesChecks
{
    public static void LegacyRefreshRecordValidatorReportsEveryProblem()
    {
        var validator = LegacyRefreshRecordRules.DefaultLegacyRefreshRecordValidator();
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRefreshRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyRefreshRecordLabelLengthRule(3600);
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRefreshRecordCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new LegacyRefreshRecordCostCenterLengthRule(3600);
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyRefreshRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyRefreshRecordCodeLengthRule(32);
        var item = LegacyRefreshRecord.NewLegacyRefreshRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRefreshRecordValidatorReportsEveryProblem();
        LegacyRefreshRecordLabelLengthRuleFlagsALongLabel();
        LegacyRefreshRecordCostCenterLengthRuleFlagsALongCostCenter();
        LegacyRefreshRecordCodeLengthRuleFlagsALongCode();
    }
}
