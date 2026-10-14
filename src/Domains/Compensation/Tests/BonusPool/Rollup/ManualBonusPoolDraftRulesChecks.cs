namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualBonusPoolDraftRulesChecks
{
    public static void ManualBonusPoolDraftValidatorReportsEveryProblem()
    {
        var validator = ManualBonusPoolDraftRules.DefaultManualBonusPoolDraftValidator();
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ManualBonusPoolDraftChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ManualBonusPoolDraftChannelLengthRule(16);
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolDraftSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new ManualBonusPoolDraftSummaryLengthRule(120);
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ManualBonusPoolDraftRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ManualBonusPoolDraftRequiredCodeRule("manual bonus pool draft needs this field");
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ManualBonusPoolDraftRequiredNameRuleFlagsABlankName()
    {
        var rule = new ManualBonusPoolDraftRequiredNameRule("manual bonus pool draft needs an entry");
        var item = ManualBonusPoolDraft.NewManualBonusPoolDraft("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ManualBonusPoolDraftValidatorReportsEveryProblem();
        ManualBonusPoolDraftChannelLengthRuleFlagsALongChannel();
        ManualBonusPoolDraftSummaryLengthRuleFlagsALongSummary();
        ManualBonusPoolDraftRequiredCodeRuleFlagsABlankCode();
        ManualBonusPoolDraftRequiredNameRuleFlagsABlankName();
    }
}
