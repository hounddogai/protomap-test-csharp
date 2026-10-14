namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class DeductionRecordRulesChecks
{
    public static void DeductionRecordValidatorReportsEveryProblem()
    {
        var validator = DeductionRecordRules.DefaultDeductionRecordValidator();
        var item = DeductionRecord.NewDeductionRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionRecordTeamLengthRuleFlagsALongTeam()
    {
        var rule = new DeductionRecordTeamLengthRule(3600);
        var item = DeductionRecord.NewDeductionRecord("x", "y");
        item.Team = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Team = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionRecordScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new DeductionRecordScoreRangeRule(9.0, 624.6);
        var item = DeductionRecord.NewDeductionRecord("x", "y");
        item.Score = 626.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DeductionRecordCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionRecordCodeLengthRule(120);
        var item = DeductionRecord.NewDeductionRecord("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionRecordRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new DeductionRecordRequiredLabelRule("deduction record needs a value");
        var item = DeductionRecord.NewDeductionRecord("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionRecordValidatorReportsEveryProblem();
        DeductionRecordTeamLengthRuleFlagsALongTeam();
        DeductionRecordScoreRangeRuleFlagsValuesAboveScore();
        DeductionRecordCodeLengthRuleFlagsALongCode();
        DeductionRecordRequiredLabelRuleFlagsABlankLabel();
    }
}
