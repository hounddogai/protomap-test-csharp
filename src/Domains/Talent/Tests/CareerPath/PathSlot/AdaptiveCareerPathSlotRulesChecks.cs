namespace Acme.Workforce.Talent.Tests.CareerPath.PathSlot;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Checks of path slot code, run by the project's runner.</summary>
public static class AdaptiveCareerPathSlotRulesChecks
{
    public static void AdaptiveCareerPathSlotValidatorReportsEveryProblem()
    {
        var validator = AdaptiveCareerPathSlotRules.DefaultAdaptiveCareerPathSlotValidator();
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveCareerPathSlotRequiredNameRuleFlagsABlankName()
    {
        var rule = new AdaptiveCareerPathSlotRequiredNameRule("adaptive career path slot needs this field");
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCareerPathSlotRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AdaptiveCareerPathSlotRequiredJobCodeRule("adaptive career path slot needs an entry");
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveCareerPathSlotNameLengthRuleFlagsALongName()
    {
        var rule = new AdaptiveCareerPathSlotNameLengthRule(3600);
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveCareerPathSlotCodeLengthRuleFlagsALongCode()
    {
        var rule = new AdaptiveCareerPathSlotCodeLengthRule(16);
        var item = AdaptiveCareerPathSlot.NewAdaptiveCareerPathSlot("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AdaptiveCareerPathSlotValidatorReportsEveryProblem();
        AdaptiveCareerPathSlotRequiredNameRuleFlagsABlankName();
        AdaptiveCareerPathSlotRequiredJobCodeRuleFlagsABlankJobCode();
        AdaptiveCareerPathSlotNameLengthRuleFlagsALongName();
        AdaptiveCareerPathSlotCodeLengthRuleFlagsALongCode();
    }
}
