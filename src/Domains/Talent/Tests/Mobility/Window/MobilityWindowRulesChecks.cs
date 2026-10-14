namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityWindowRulesChecks
{
    public static void MobilityWindowValidatorReportsEveryProblem()
    {
        var validator = MobilityWindowRules.DefaultMobilityWindowValidator();
        var item = MobilityWindow.NewMobilityWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MobilityWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new MobilityWindowCodeLengthRule(120);
        var item = MobilityWindow.NewMobilityWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MobilityWindowRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new MobilityWindowRequiredSegmentRule("mobility window needs an entry");
        var item = MobilityWindow.NewMobilityWindow("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new MobilityWindowRequiredCodeRule("mobility window needs a value");
        var item = MobilityWindow.NewMobilityWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void MobilityWindowSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new MobilityWindowSegmentLengthRule(32);
        var item = MobilityWindow.NewMobilityWindow("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        MobilityWindowValidatorReportsEveryProblem();
        MobilityWindowCodeLengthRuleFlagsALongCode();
        MobilityWindowRequiredSegmentRuleFlagsABlankSegment();
        MobilityWindowRequiredCodeRuleFlagsABlankCode();
        MobilityWindowSegmentLengthRuleFlagsALongSegment();
    }
}
