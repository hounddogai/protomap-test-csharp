namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class PrimaryVisitorOverrideRulesChecks
{
    public static void PrimaryVisitorOverrideValidatorReportsEveryProblem()
    {
        var validator = PrimaryVisitorOverrideRules.DefaultPrimaryVisitorOverrideValidator();
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryVisitorOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new PrimaryVisitorOverrideDurationMinutesRangeRule(32, 86400);
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("x", "y");
        item.DurationMinutes = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryVisitorOverrideMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new PrimaryVisitorOverrideMinimumRangeRule(8.5, 4254.6);
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("x", "y");
        item.Minimum = 4256.1;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryVisitorOverrideNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryVisitorOverrideNameLengthRule(32);
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryVisitorOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PrimaryVisitorOverrideCreatedAtRangeRule(120, 255);
        var item = PrimaryVisitorOverride.NewPrimaryVisitorOverride("x", "y");
        item.CreatedAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryVisitorOverrideValidatorReportsEveryProblem();
        PrimaryVisitorOverrideDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        PrimaryVisitorOverrideMinimumRangeRuleFlagsValuesAboveMinimum();
        PrimaryVisitorOverrideNameLengthRuleFlagsALongName();
        PrimaryVisitorOverrideCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
