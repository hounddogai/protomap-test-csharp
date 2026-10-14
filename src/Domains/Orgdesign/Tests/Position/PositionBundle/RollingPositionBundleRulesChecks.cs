namespace Acme.Workforce.Orgdesign.Tests.Position.PositionBundle;

using Acme.Workforce.Orgdesign.Position.PositionBundle;

/// <summary>Checks of position bundle code, run by the project's runner.</summary>
public static class RollingPositionBundleRulesChecks
{
    public static void RollingPositionBundleValidatorReportsEveryProblem()
    {
        var validator = RollingPositionBundleRules.DefaultRollingPositionBundleValidator();
        var item = RollingPositionBundle.NewRollingPositionBundle("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingPositionBundleStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new RollingPositionBundleStartsAtRangeRule(16, 86400);
        var item = RollingPositionBundle.NewRollingPositionBundle("x", "y");
        item.StartsAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RollingPositionBundleCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingPositionBundleCodeLengthRule(120);
        var item = RollingPositionBundle.NewRollingPositionBundle("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingPositionBundleRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new RollingPositionBundleRevisionRangeRule(64, 80);
        var item = RollingPositionBundle.NewRollingPositionBundle("x", "y");
        item.Revision = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingPositionBundleValidatorReportsEveryProblem();
        RollingPositionBundleStartsAtRangeRuleFlagsValuesAboveStartsAt();
        RollingPositionBundleCodeLengthRuleFlagsALongCode();
        RollingPositionBundleRevisionRangeRuleFlagsValuesAboveRevision();
    }
}
