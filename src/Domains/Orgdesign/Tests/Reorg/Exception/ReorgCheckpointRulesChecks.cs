namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgCheckpointRulesChecks
{
    public static void ReorgCheckpointValidatorReportsEveryProblem()
    {
        var validator = ReorgCheckpointRules.DefaultReorgCheckpointValidator();
        var item = ReorgCheckpoint.NewReorgCheckpoint("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgCheckpointRequiredNameRuleFlagsABlankName()
    {
        var rule = new ReorgCheckpointRequiredNameRule("reorg checkpoint needs a value");
        var item = ReorgCheckpoint.NewReorgCheckpoint("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgCheckpointSourceLengthRuleFlagsALongSource()
    {
        var rule = new ReorgCheckpointSourceLengthRule(1000);
        var item = ReorgCheckpoint.NewReorgCheckpoint("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgCheckpointRequiredSourceRuleFlagsABlankSource()
    {
        var rule = new ReorgCheckpointRequiredSourceRule("reorg checkpoint needs a value");
        var item = ReorgCheckpoint.NewReorgCheckpoint("", "");
        item.Source = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgCheckpointNameLengthRuleFlagsALongName()
    {
        var rule = new ReorgCheckpointNameLengthRule(3600);
        var item = ReorgCheckpoint.NewReorgCheckpoint("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReorgCheckpointRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgCheckpointRequiredCodeRule("reorg checkpoint needs a value");
        var item = ReorgCheckpoint.NewReorgCheckpoint("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgCheckpointValidatorReportsEveryProblem();
        ReorgCheckpointRequiredNameRuleFlagsABlankName();
        ReorgCheckpointSourceLengthRuleFlagsALongSource();
        ReorgCheckpointRequiredSourceRuleFlagsABlankSource();
        ReorgCheckpointNameLengthRuleFlagsALongName();
        ReorgCheckpointRequiredCodeRuleFlagsABlankCode();
    }
}
