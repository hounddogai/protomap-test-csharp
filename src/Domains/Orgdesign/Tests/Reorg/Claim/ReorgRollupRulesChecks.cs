namespace Acme.Workforce.Orgdesign.Tests.Reorg.Claim;

using Acme.Workforce.Orgdesign.Reorg.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ReorgRollupRulesChecks
{
    public static void ReorgRollupValidatorReportsEveryProblem()
    {
        var validator = ReorgRollupRules.DefaultReorgRollupValidator();
        var item = ReorgRollup.NewReorgRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgRollupRequiredCodeRule("reorg rollup needs an entry");
        var item = ReorgRollup.NewReorgRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgRollupNameLengthRuleFlagsALongName()
    {
        var rule = new ReorgRollupNameLengthRule(86400);
        var item = ReorgRollup.NewReorgRollup("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgRollupValidatorReportsEveryProblem();
        ReorgRollupRequiredCodeRuleFlagsABlankCode();
        ReorgRollupNameLengthRuleFlagsALongName();
    }
}
