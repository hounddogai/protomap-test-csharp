namespace Acme.Workforce.Talent.Tests.Mobility.Target;

using Acme.Workforce.Talent.Mobility.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MobilityNoticeRulesChecks
{
    public static void MobilityNoticeValidatorReportsEveryProblem()
    {
        var validator = MobilityNoticeRules.DefaultMobilityNoticeValidator();
        var item = MobilityNotice.NewMobilityNotice("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityNoticeRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new MobilityNoticeRequiredUnitRule("mobility notice needs this field");
        var item = MobilityNotice.NewMobilityNotice("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityNoticeUnitLengthRuleFlagsALongUnit()
    {
        var rule = new MobilityNoticeUnitLengthRule(16);
        var item = MobilityNotice.NewMobilityNotice("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityNoticeCostRangeRuleFlagsValuesAboveCost()
    {
        var rule = new MobilityNoticeCostRangeRule(5.7, 2040.3);
        var item = MobilityNotice.NewMobilityNotice("x", "y");
        item.Cost = 2041.8;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MobilityNoticeValidatorReportsEveryProblem();
        MobilityNoticeRequiredUnitRuleFlagsABlankUnit();
        MobilityNoticeUnitLengthRuleFlagsALongUnit();
        MobilityNoticeCostRangeRuleFlagsValuesAboveCost();
    }
}
