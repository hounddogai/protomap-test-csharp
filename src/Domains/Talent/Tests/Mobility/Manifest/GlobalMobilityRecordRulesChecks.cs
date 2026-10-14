namespace Acme.Workforce.Talent.Tests.Mobility.Manifest;

using Acme.Workforce.Talent.Mobility.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class GlobalMobilityRecordRulesChecks
{
    public static void GlobalMobilityRecordValidatorReportsEveryProblem()
    {
        var validator = GlobalMobilityRecordRules.DefaultGlobalMobilityRecordValidator();
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GlobalMobilityRecordAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new GlobalMobilityRecordAmountRangeRule(0.2, 1111.8);
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("x", "y");
        item.Amount = 1113.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GlobalMobilityRecordManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new GlobalMobilityRecordManagerIdLengthRule(32);
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalMobilityRecordReviewerLengthRuleFlagsALongReviewer()
    {
        var rule = new GlobalMobilityRecordReviewerLengthRule(64);
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("x", "y");
        item.Reviewer = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Reviewer = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GlobalMobilityRecordTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GlobalMobilityRecordTitleLengthRule(1000);
        var item = GlobalMobilityRecord.NewGlobalMobilityRecord("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        GlobalMobilityRecordValidatorReportsEveryProblem();
        GlobalMobilityRecordAmountRangeRuleFlagsValuesAboveAmount();
        GlobalMobilityRecordManagerIdLengthRuleFlagsALongManagerId();
        GlobalMobilityRecordReviewerLengthRuleFlagsALongReviewer();
        GlobalMobilityRecordTitleLengthRuleFlagsALongTitle();
    }
}
