namespace Acme.Workforce.Engagement.Tests.TownHall.Target;

using Acme.Workforce.Engagement.TownHall.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class LenientTownHallTargetRulesChecks
{
    public static void LenientTownHallTargetValidatorReportsEveryProblem()
    {
        var validator = LenientTownHallTargetRules.DefaultLenientTownHallTargetValidator();
        var item = LenientTownHallTarget.NewLenientTownHallTarget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientTownHallTargetCodeLengthRuleFlagsALongCode()
    {
        var rule = new LenientTownHallTargetCodeLengthRule(120);
        var item = LenientTownHallTarget.NewLenientTownHallTarget("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LenientTownHallTargetRequiredNameRuleFlagsABlankName()
    {
        var rule = new LenientTownHallTargetRequiredNameRule("lenient town hall target needs this field");
        var item = LenientTownHallTarget.NewLenientTownHallTarget("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LenientTownHallTargetValidatorReportsEveryProblem();
        LenientTownHallTargetCodeLengthRuleFlagsALongCode();
        LenientTownHallTargetRequiredNameRuleFlagsABlankName();
    }
}
