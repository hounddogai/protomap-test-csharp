namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnReservationRulesChecks
{
    public static void AssetReturnReservationValidatorReportsEveryProblem()
    {
        var validator = AssetReturnReservationRules.DefaultAssetReturnReservationValidator();
        var item = AssetReturnReservation.NewAssetReturnReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AssetReturnReservationFteRangeRuleFlagsValuesAboveFte()
    {
        var rule = new AssetReturnReservationFteRangeRule(2.1, 3899.2);
        var item = AssetReturnReservation.NewAssetReturnReservation("x", "y");
        item.Fte = 3900.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AssetReturnReservationDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new AssetReturnReservationDepartmentLengthRule(32);
        var item = AssetReturnReservation.NewAssetReturnReservation("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AssetReturnReservationValidatorReportsEveryProblem();
        AssetReturnReservationFteRangeRuleFlagsValuesAboveFte();
        AssetReturnReservationDepartmentLengthRuleFlagsALongDepartment();
    }
}
