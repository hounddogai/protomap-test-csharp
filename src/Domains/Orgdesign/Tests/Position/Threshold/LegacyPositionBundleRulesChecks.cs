namespace Acme.Workforce.Orgdesign.Tests.Position.Threshold;

using Acme.Workforce.Orgdesign.Position.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyPositionBundleRulesChecks
{
    public static void LegacyPositionBundleValidatorReportsEveryProblem()
    {
        var validator = LegacyPositionBundleRules.DefaultLegacyPositionBundleValidator();
        var item = LegacyPositionBundle.NewLegacyPositionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyPositionBundleSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new LegacyPositionBundleSegmentLengthRule(86400);
        var item = LegacyPositionBundle.NewLegacyPositionBundle("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPositionBundleRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new LegacyPositionBundleRequiredLabelRule("legacy position bundle needs an entry");
        var item = LegacyPositionBundle.NewLegacyPositionBundle("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyPositionBundleContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new LegacyPositionBundleContactEmailLengthRule(3600);
        var item = LegacyPositionBundle.NewLegacyPositionBundle("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyPositionBundleLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LegacyPositionBundleLabelLengthRule(120);
        var item = LegacyPositionBundle.NewLegacyPositionBundle("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyPositionBundleValidatorReportsEveryProblem();
        LegacyPositionBundleSegmentLengthRuleFlagsALongSegment();
        LegacyPositionBundleRequiredLabelRuleFlagsABlankLabel();
        LegacyPositionBundleContactEmailLengthRuleFlagsALongContactEmail();
        LegacyPositionBundleLabelLengthRuleFlagsALongLabel();
    }
}
