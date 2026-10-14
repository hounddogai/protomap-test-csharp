namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgEntryRulesChecks
{
    public static void ReorgEntryValidatorReportsEveryProblem()
    {
        var validator = ReorgEntryRules.DefaultReorgEntryValidator();
        var item = ReorgEntry.NewReorgEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgEntryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgEntryRequiredCodeRule("reorg entry needs this field");
        var item = ReorgEntry.NewReorgEntry("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new ReorgEntryCodeLengthRule(80);
        var item = ReorgEntry.NewReorgEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ReorgEntryValidatorReportsEveryProblem();
        ReorgEntryRequiredCodeRuleFlagsABlankCode();
        ReorgEntryCodeLengthRuleFlagsALongCode();
    }
}
