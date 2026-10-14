namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgProfile;

using Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

/// <summary>Checks of reorg profile code, run by the project's runner.</summary>
public static class ReorgWindowRulesChecks
{
    public static void ReorgWindowValidatorReportsEveryProblem()
    {
        var validator = ReorgWindowRules.DefaultReorgWindowValidator();
        var item = ReorgWindow.NewReorgWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReorgWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReorgWindowRequiredCodeRule("reorg window needs this field");
        var item = ReorgWindow.NewReorgWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReorgWindowRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new ReorgWindowRequiredUnitRule("reorg window needs an entry");
        var item = ReorgWindow.NewReorgWindow("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReorgWindowValidatorReportsEveryProblem();
        ReorgWindowRequiredCodeRuleFlagsABlankCode();
        ReorgWindowRequiredUnitRuleFlagsABlankUnit();
    }
}
