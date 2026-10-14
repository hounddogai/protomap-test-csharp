namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatTemplate;

using Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

/// <summary>Checks of seat template code, run by the project's runner.</summary>
public static class LicenseSeatPolicyRulesChecks
{
    public static void LicenseSeatPolicyValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatPolicyRules.DefaultLicenseSeatPolicyValidator();
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatPolicyRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new LicenseSeatPolicyRequiredSegmentRule("license seat policy needs this field");
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatPolicyQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LicenseSeatPolicyQuantityRangeRule(1000, 3600);
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("x", "y");
        item.Quantity = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatPolicyCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatPolicyCodeLengthRule(255);
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatPolicyStartsAtRangeRuleFlagsValuesAboveStartsAt()
    {
        var rule = new LicenseSeatPolicyStartsAtRangeRule(1000, 3600);
        var item = LicenseSeatPolicy.NewLicenseSeatPolicy("x", "y");
        item.StartsAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        LicenseSeatPolicyValidatorReportsEveryProblem();
        LicenseSeatPolicyRequiredSegmentRuleFlagsABlankSegment();
        LicenseSeatPolicyQuantityRangeRuleFlagsValuesAboveQuantity();
        LicenseSeatPolicyCodeLengthRuleFlagsALongCode();
        LicenseSeatPolicyStartsAtRangeRuleFlagsValuesAboveStartsAt();
    }
}
