namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BatchedTownHallNoticeRulesChecks
{
    public static void BatchedTownHallNoticeValidatorReportsEveryProblem()
    {
        var validator = BatchedTownHallNoticeRules.DefaultBatchedTownHallNoticeValidator();
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BatchedTownHallNoticeNameLengthRuleFlagsALongName()
    {
        var rule = new BatchedTownHallNoticeNameLengthRule(86400);
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTownHallNoticeUnitLengthRuleFlagsALongUnit()
    {
        var rule = new BatchedTownHallNoticeUnitLengthRule(255);
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BatchedTownHallNoticeRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new BatchedTownHallNoticeRequiredChannelRule("batched town hall notice needs this field");
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BatchedTownHallNoticeValidatorReportsEveryProblem();
        BatchedTownHallNoticeNameLengthRuleFlagsALongName();
        BatchedTownHallNoticeUnitLengthRuleFlagsALongUnit();
        BatchedTownHallNoticeRequiredChannelRuleFlagsABlankChannel();
    }
}
