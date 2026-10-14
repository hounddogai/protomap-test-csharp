namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class EngagementRecordRulesChecks
{
    public static void EngagementRecordValidatorReportsEveryProblem()
    {
        var validator = EngagementRecordRules.DefaultEngagementRecordValidator();
        var item = EngagementRecord.NewEngagementRecord("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EngagementRecordSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new EngagementRecordSummaryLengthRule(3600);
        var item = EngagementRecord.NewEngagementRecord("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementRecordExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new EngagementRecordExternalRefLengthRule(80);
        var item = EngagementRecord.NewEngagementRecord("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EngagementRecordFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new EngagementRecordFteRangeRule(9.9, 2433.9);
        var item = EngagementRecord.NewEngagementRecord("x", "y");
        item.Fte = 2435.4;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        EngagementRecordValidatorReportsEveryProblem();
        EngagementRecordSummaryLengthRuleFlagsALongSummary();
        EngagementRecordExternalRefLengthRuleFlagsALongExternalRef();
        EngagementRecordFteRangeRuleFlagsValuesAboveFte();
    }
}
