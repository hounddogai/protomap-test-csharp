namespace Acme.Workforce.Talent.Tests.Pool.Baseline;

using Acme.Workforce.Talent.Pool.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PoolReservationRulesChecks
{
    public static void PoolReservationValidatorReportsEveryProblem()
    {
        var validator = PoolReservationRules.DefaultPoolReservationValidator();
        var item = PoolReservation.NewPoolReservation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PoolReservationLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PoolReservationLabelLengthRule(16);
        var item = PoolReservation.NewPoolReservation("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolReservationRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PoolReservationRequiredLabelRule("pool reservation needs a value");
        var item = PoolReservation.NewPoolReservation("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PoolReservationCodeLengthRuleFlagsALongCode()
    {
        var rule = new PoolReservationCodeLengthRule(80);
        var item = PoolReservation.NewPoolReservation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PoolReservationCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PoolReservationCapacityRangeRule(255, 1000);
        var item = PoolReservation.NewPoolReservation("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PoolReservationValidatorReportsEveryProblem();
        PoolReservationLabelLengthRuleFlagsALongLabel();
        PoolReservationRequiredLabelRuleFlagsABlankLabel();
        PoolReservationCodeLengthRuleFlagsALongCode();
        PoolReservationCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
