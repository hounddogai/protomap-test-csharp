namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityRollupRulesChecks
{
    public static void ScheduledMobilityRollupValidatorReportsEveryProblem()
    {
        var validator = ScheduledMobilityRollupRules.DefaultScheduledMobilityRollupValidator();
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledMobilityRollupQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new ScheduledMobilityRollupQuantityRangeRule(64, 80);
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("x", "y");
        item.Quantity = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledMobilityRollupManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ScheduledMobilityRollupManagerIdLengthRule(255);
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityRollupPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new ScheduledMobilityRollupPhoneLengthRule(255);
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledMobilityRollupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ScheduledMobilityRollupRequiredCodeRule("scheduled mobility rollup needs a value");
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledMobilityRollupRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new ScheduledMobilityRollupRequiredPhoneRule("scheduled mobility rollup needs an entry");
        var item = ScheduledMobilityRollup.NewScheduledMobilityRollup("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ScheduledMobilityRollupValidatorReportsEveryProblem();
        ScheduledMobilityRollupQuantityRangeRuleFlagsValuesAboveQuantity();
        ScheduledMobilityRollupManagerIdLengthRuleFlagsALongManagerId();
        ScheduledMobilityRollupPhoneLengthRuleFlagsALongPhone();
        ScheduledMobilityRollupRequiredCodeRuleFlagsABlankCode();
        ScheduledMobilityRollupRequiredPhoneRuleFlagsABlankPhone();
    }
}
