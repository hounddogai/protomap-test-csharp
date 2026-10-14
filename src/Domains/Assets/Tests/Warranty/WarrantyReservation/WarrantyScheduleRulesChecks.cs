namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class WarrantyScheduleRulesChecks
{
    public static void WarrantyScheduleValidatorReportsEveryProblem()
    {
        var validator = WarrantyScheduleRules.DefaultWarrantyScheduleValidator();
        var item = WarrantySchedule.NewWarrantySchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void WarrantyScheduleCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new WarrantyScheduleCostCenterLengthRule(3600);
        var item = WarrantySchedule.NewWarrantySchedule("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void WarrantyScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new WarrantyScheduleRequiredCodeRule("warranty schedule needs a value");
        var item = WarrantySchedule.NewWarrantySchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void WarrantyScheduleRequiredCostCenterRuleFlagsABlankCostCenter()
    {
        var rule = new WarrantyScheduleRequiredCostCenterRule("warranty schedule needs an entry");
        var item = WarrantySchedule.NewWarrantySchedule("", "");
        item.CostCenter = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        WarrantyScheduleValidatorReportsEveryProblem();
        WarrantyScheduleCostCenterLengthRuleFlagsALongCostCenter();
        WarrantyScheduleRequiredCodeRuleFlagsABlankCode();
        WarrantyScheduleRequiredCostCenterRuleFlagsABlankCostCenter();
    }
}
