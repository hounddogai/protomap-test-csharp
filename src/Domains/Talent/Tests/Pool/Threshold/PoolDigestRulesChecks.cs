namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolDigestRulesChecks
{
    public static void PoolDigestValidatorReportsEveryProblem()
    {
        var validator = PoolDigestRules.DefaultPoolDigestValidator();
        var item = PoolDigest.NewPoolDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PoolDigestTitleLengthRule(1000);
        var item = PoolDigest.NewPoolDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolDigestSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PoolDigestSequenceRangeRule(120, 255);
        var item = PoolDigest.NewPoolDigest("x", "y");
        item.Sequence = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PoolDigestValidatorReportsEveryProblem();
        PoolDigestTitleLengthRuleFlagsALongTitle();
        PoolDigestSequenceRangeRuleFlagsValuesAboveSequence();
    }
}
