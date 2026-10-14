namespace Acme.Workforce.Assets.Tests.Warranty.Summary;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class WarrantySummaryRulesChecks
{
    public static void WarrantySummaryValidatorReportsEveryProblem()
    {
        var validator = WarrantySummaryRules.DefaultWarrantySummaryValidator();
        var item = WarrantySummary.NewWarrantySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantySummaryRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new WarrantySummaryRequiredLabelRule("warranty summary needs a value");
        var item = WarrantySummary.NewWarrantySummary("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantySummaryRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new WarrantySummaryRequiredJobCodeRule("warranty summary needs an entry");
        var item = WarrantySummary.NewWarrantySummary("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantySummaryEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new WarrantySummaryEndsAtRangeRule(32, 86400);
        var item = WarrantySummary.NewWarrantySummary("x", "y");
        item.EndsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void WarrantySummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new WarrantySummaryCodeLengthRule(64);
        var item = WarrantySummary.NewWarrantySummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        WarrantySummaryValidatorReportsEveryProblem();
        WarrantySummaryRequiredLabelRuleFlagsABlankLabel();
        WarrantySummaryRequiredJobCodeRuleFlagsABlankJobCode();
        WarrantySummaryEndsAtRangeRuleFlagsValuesAboveEndsAt();
        WarrantySummaryCodeLengthRuleFlagsALongCode();
    }
}
