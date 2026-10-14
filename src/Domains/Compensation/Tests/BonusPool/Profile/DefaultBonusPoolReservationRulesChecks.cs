namespace Acme.Workforce.Compensation.Tests.BonusPool.Profile;

using Acme.Workforce.Compensation.BonusPool.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class DefaultBonusPoolReservationRulesChecks
{
    public static void DefaultBonusPoolReservationValidatorReportsEveryProblem()
    {
        var validator = DefaultBonusPoolReservationRules.DefaultDefaultBonusPoolReservationValidator();
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DefaultBonusPoolReservationRequiredNameRuleFlagsABlankName()
    {
        var rule = new DefaultBonusPoolReservationRequiredNameRule("default bonus pool reservation needs a value");
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBonusPoolReservationFloorValueRangeRuleFlagsValuesAboveFloorValue()
    {
        var rule = new DefaultBonusPoolReservationFloorValueRangeRule(3.7, 3998.1);
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("x", "y");
        item.FloorValue = 3999.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void DefaultBonusPoolReservationRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new DefaultBonusPoolReservationRequiredCostCenterRule(
            "default bonus pool reservation needs an entry");
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBonusPoolReservationRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new DefaultBonusPoolReservationRequiredCodeRule("default bonus pool reservation needs an entry");
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBonusPoolReservationRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new DefaultBonusPoolReservationRequiredUnitRule("default bonus pool reservation needs a value");
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DefaultBonusPoolReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new DefaultBonusPoolReservationCodeLengthRule(16);
        var item = DefaultBonusPoolReservation.NewDefaultBonusPoolReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        DefaultBonusPoolReservationValidatorReportsEveryProblem();
        DefaultBonusPoolReservationRequiredNameRuleFlagsABlankName();
        DefaultBonusPoolReservationFloorValueRangeRuleFlagsValuesAboveFloorValue();
        DefaultBonusPoolReservationRequiredCostCenterRuleFlagsABlankCostCenter();
        DefaultBonusPoolReservationRequiredCodeRuleFlagsABlankCode();
        DefaultBonusPoolReservationRequiredUnitRuleFlagsABlankUnit();
        DefaultBonusPoolReservationCodeLengthRuleFlagsALongCode();
    }
}
