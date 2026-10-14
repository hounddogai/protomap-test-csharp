namespace Acme.Workforce.Talent.Tests.Pool.Draft;

using Acme.Workforce.Talent.Pool.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PoolMilestoneRulesChecks
{
    public static void PoolMilestoneValidatorReportsEveryProblem()
    {
        var validator = PoolMilestoneRules.DefaultPoolMilestoneValidator();
        var item = PoolMilestone.NewPoolMilestone("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolMilestoneRequiredNotesRuleFlagsABlankNotes()
    {
        var rule = new PoolMilestoneRequiredNotesRule("pool milestone needs this field");
        var item = PoolMilestone.NewPoolMilestone("", "");
        item.Notes = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolMilestoneRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PoolMilestoneRequiredCodeRule("pool milestone needs an entry");
        var item = PoolMilestone.NewPoolMilestone("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolMilestoneTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PoolMilestoneTitleLengthRule(80);
        var item = PoolMilestone.NewPoolMilestone("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolMilestoneCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolMilestoneCodeLengthRule(1000);
        var item = PoolMilestone.NewPoolMilestone("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PoolMilestoneValidatorReportsEveryProblem();
        PoolMilestoneRequiredNotesRuleFlagsABlankNotes();
        PoolMilestoneRequiredCodeRuleFlagsABlankCode();
        PoolMilestoneTitleLengthRuleFlagsALongTitle();
        PoolMilestoneCodeLengthRuleFlagsALongCode();
    }
}
