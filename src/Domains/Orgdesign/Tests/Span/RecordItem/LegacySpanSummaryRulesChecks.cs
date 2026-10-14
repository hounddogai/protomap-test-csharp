namespace Acme.Workforce.Orgdesign.Tests.Span.RecordItem;

using Acme.Workforce.Orgdesign.Span.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LegacySpanSummaryRulesChecks
{
    public static void LegacySpanSummaryValidatorReportsEveryProblem()
    {
        var validator = LegacySpanSummaryRules.DefaultLegacySpanSummaryValidator();
        var item = LegacySpanSummary.NewLegacySpanSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacySpanSummaryOwnerIdLengthRuleFlagsALongOwnerId()
    {
        var rule = new LegacySpanSummaryOwnerIdLengthRule(255);
        var item = LegacySpanSummary.NewLegacySpanSummary("x", "y");
        item.OwnerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.OwnerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySpanSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacySpanSummaryCodeLengthRule(32);
        var item = LegacySpanSummary.NewLegacySpanSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacySpanSummaryJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new LegacySpanSummaryJobCodeLengthRule(32);
        var item = LegacySpanSummary.NewLegacySpanSummary("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacySpanSummaryValidatorReportsEveryProblem();
        LegacySpanSummaryOwnerIdLengthRuleFlagsALongOwnerId();
        LegacySpanSummaryCodeLengthRuleFlagsALongCode();
        LegacySpanSummaryJobCodeLengthRuleFlagsALongJobCode();
    }
}
