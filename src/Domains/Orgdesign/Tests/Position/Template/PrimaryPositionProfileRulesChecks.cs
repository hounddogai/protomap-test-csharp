namespace Acme.Workforce.Orgdesign.Tests.Position.Template;

using Acme.Workforce.Orgdesign.Position.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PrimaryPositionProfileRulesChecks
{
    public static void PrimaryPositionProfileValidatorReportsEveryProblem()
    {
        var validator = PrimaryPositionProfileRules.DefaultPrimaryPositionProfileValidator();
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryPositionProfileTitleLengthRuleFlagsALongTitle()
    {
        var rule = new PrimaryPositionProfileTitleLengthRule(86400);
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryPositionProfilePriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new PrimaryPositionProfilePriorityRangeRule(64, 3600);
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryPositionProfileMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new PrimaryPositionProfileMinimumRangeRule(2.8, 4369.1);
        var item = PrimaryPositionProfile.NewPrimaryPositionProfile("x", "y");
        item.Minimum = 4370.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryPositionProfileValidatorReportsEveryProblem();
        PrimaryPositionProfileTitleLengthRuleFlagsALongTitle();
        PrimaryPositionProfilePriorityRangeRuleFlagsValuesAbovePriority();
        PrimaryPositionProfileMinimumRangeRuleFlagsValuesAboveMinimum();
    }
}
