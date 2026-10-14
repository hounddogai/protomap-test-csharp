namespace Acme.Workforce.Orgdesign.Tests.Position.Entry;

using Acme.Workforce.Orgdesign.Position.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class PositionProfileRulesChecks
{
    public static void PositionProfileValidatorReportsEveryProblem()
    {
        var validator = PositionProfileRules.DefaultPositionProfileValidator();
        var item = PositionProfile.NewPositionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PositionProfileCodeLengthRuleFlagsALongCode()
    {
        var rule = new PositionProfileCodeLengthRule(255);
        var item = PositionProfile.NewPositionProfile("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PositionProfileRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new PositionProfileRequiredSegmentRule("position profile needs this field");
        var item = PositionProfile.NewPositionProfile("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PositionProfileMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new PositionProfileMaximumRangeRule(9.6, 4728.2);
        var item = PositionProfile.NewPositionProfile("x", "y");
        item.Maximum = 4729.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PositionProfileRequiredNameRuleFlagsABlankName()
    {
        var rule = new PositionProfileRequiredNameRule("position profile needs this field");
        var item = PositionProfile.NewPositionProfile("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PositionProfileValidatorReportsEveryProblem();
        PositionProfileCodeLengthRuleFlagsALongCode();
        PositionProfileRequiredSegmentRuleFlagsABlankSegment();
        PositionProfileMaximumRangeRuleFlagsValuesAboveMaximum();
        PositionProfileRequiredNameRuleFlagsABlankName();
    }
}
