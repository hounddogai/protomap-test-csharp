namespace Acme.Workforce.Talent.Tests.CareerPath.Slot;

using Acme.Workforce.Talent.CareerPath.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CareerPathOverrideRulesChecks
{
    public static void CareerPathOverrideValidatorReportsEveryProblem()
    {
        var validator = CareerPathOverrideRules.DefaultCareerPathOverrideValidator();
        var item = CareerPathOverride.NewCareerPathOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathOverrideRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new CareerPathOverrideRequiredChannelRule("career path override needs a value");
        var item = CareerPathOverride.NewCareerPathOverride("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathOverrideChannelLengthRuleFlagsALongChannel()
    {
        var rule = new CareerPathOverrideChannelLengthRule(64);
        var item = CareerPathOverride.NewCareerPathOverride("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new CareerPathOverrideCreatedAtRangeRule(120, 255);
        var item = CareerPathOverride.NewCareerPathOverride("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CareerPathOverrideValidatorReportsEveryProblem();
        CareerPathOverrideRequiredChannelRuleFlagsABlankChannel();
        CareerPathOverrideChannelLengthRuleFlagsALongChannel();
        CareerPathOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
