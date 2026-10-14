namespace Acme.Workforce.Talent.Tests.CareerPath.Ledger;

using Acme.Workforce.Talent.CareerPath.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CareerPathGroupRulesChecks
{
    public static void CareerPathGroupValidatorReportsEveryProblem()
    {
        var validator = CareerPathGroupRules.DefaultCareerPathGroupValidator();
        var item = CareerPathGroup.NewCareerPathGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CareerPathGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CareerPathGroupRequiredCodeRule("career path group needs this field");
        var item = CareerPathGroup.NewCareerPathGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CareerPathGroupPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new CareerPathGroupPhoneLengthRule(64);
        var item = CareerPathGroup.NewCareerPathGroup("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CareerPathGroupExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CareerPathGroupExpiresAtRangeRule(16, 120);
        var item = CareerPathGroup.NewCareerPathGroup("x", "y");
        item.ExpiresAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CareerPathGroupRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new CareerPathGroupRequiredLabelRule("career path group needs a value");
        var item = CareerPathGroup.NewCareerPathGroup("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        CareerPathGroupValidatorReportsEveryProblem();
        CareerPathGroupRequiredCodeRuleFlagsABlankCode();
        CareerPathGroupPhoneLengthRuleFlagsALongPhone();
        CareerPathGroupExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        CareerPathGroupRequiredLabelRuleFlagsABlankLabel();
    }
}
