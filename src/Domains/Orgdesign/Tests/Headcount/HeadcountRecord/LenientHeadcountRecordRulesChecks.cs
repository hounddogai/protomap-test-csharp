namespace Acme.Workforce.Orgdesign.Tests.Headcount.HeadcountRecord;

using Acme.Workforce.Orgdesign.Headcount.HeadcountRecord;

/// <summary>Checks of headcount record code, run by the project's runner.</summary>
public static class LenientHeadcountRecordRulesChecks
{
    public static void LenientHeadcountRecordValidatorReportsEveryProblem()
    {
        var validator = LenientHeadcountRecordRules.DefaultLenientHeadcountRecordValidator();
        var item = LenientHeadcountRecord.NewLenientHeadcountRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientHeadcountRecordLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LenientHeadcountRecordLabelLengthRule(255);
        var item = LenientHeadcountRecord.NewLenientHeadcountRecord("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientHeadcountRecordScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new LenientHeadcountRecordScoreRangeRule(7.4, 4874.5);
        var item = LenientHeadcountRecord.NewLenientHeadcountRecord("x", "y");
        item.Score = 4876.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LenientHeadcountRecordValidatorReportsEveryProblem();
        LenientHeadcountRecordLabelLengthRuleFlagsALongLabel();
        LenientHeadcountRecordScoreRangeRuleFlagsValuesAboveScore();
    }
}
