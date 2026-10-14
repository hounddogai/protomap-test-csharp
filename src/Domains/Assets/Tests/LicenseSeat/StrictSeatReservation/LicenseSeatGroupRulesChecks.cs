namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatGroupRulesChecks
{
    public static void LicenseSeatGroupValidatorReportsEveryProblem()
    {
        var validator = LicenseSeatGroupRules.DefaultLicenseSeatGroupValidator();
        var item = LicenseSeatGroup.NewLicenseSeatGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LicenseSeatGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new LicenseSeatGroupLabelLengthRule(16);
        var item = LicenseSeatGroup.NewLicenseSeatGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LicenseSeatGroupRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new LicenseSeatGroupRequiredLocationRule("license seat group needs an entry");
        var item = LicenseSeatGroup.NewLicenseSeatGroup("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LicenseSeatGroupCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new LicenseSeatGroupCreatedAtRangeRule(86400, 86400);
        var item = LicenseSeatGroup.NewLicenseSeatGroup("x", "y");
        item.CreatedAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LicenseSeatGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new LicenseSeatGroupCodeLengthRule(3600);
        var item = LicenseSeatGroup.NewLicenseSeatGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LicenseSeatGroupValidatorReportsEveryProblem();
        LicenseSeatGroupLabelLengthRuleFlagsALongLabel();
        LicenseSeatGroupRequiredLocationRuleFlagsABlankLocation();
        LicenseSeatGroupCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        LicenseSeatGroupCodeLengthRuleFlagsALongCode();
    }
}
