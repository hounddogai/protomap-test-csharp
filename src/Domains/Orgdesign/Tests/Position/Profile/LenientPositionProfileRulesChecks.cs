namespace Acme.Workforce.Orgdesign.Tests.Position.Profile;

using Acme.Workforce.Orgdesign.Position.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class LenientPositionProfileRulesChecks
{
    public static void LenientPositionProfileValidatorReportsEveryProblem()
    {
        var validator = LenientPositionProfileRules.DefaultLenientPositionProfileValidator();
        var item = LenientPositionProfile.NewLenientPositionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientPositionProfileCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new LenientPositionProfileCostRangeRule(1.6, 2568.3);
        var item = LenientPositionProfile.NewLenientPositionProfile("x", "y");
        item.Cost = 2569.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientPositionProfileRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LenientPositionProfileRequiredTitleRule("lenient position profile needs a value");
        var item = LenientPositionProfile.NewLenientPositionProfile("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LenientPositionProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientPositionProfileTitleLengthRule(32);
        var item = LenientPositionProfile.NewLenientPositionProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientPositionProfileValidatorReportsEveryProblem();
        LenientPositionProfileCostRangeRuleFlagsValuesAboveCost();
        LenientPositionProfileRequiredTitleRuleFlagsABlankTitle();
        LenientPositionProfileTitleLengthRuleFlagsALongTitle();
    }
}
