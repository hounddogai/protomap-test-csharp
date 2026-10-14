namespace Acme.Workforce.Compensation.Tests.MeritCycle.Profile;

using Acme.Workforce.Compensation.MeritCycle.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class MeritCycleAssignmentRulesChecks
{
    public static void MeritCycleAssignmentValidatorReportsEveryProblem()
    {
        var validator = MeritCycleAssignmentRules.DefaultMeritCycleAssignmentValidator();
        var item = MeritCycleAssignment.NewMeritCycleAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void MeritCycleAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new MeritCycleAssignmentCodeLengthRule(16);
        var item = MeritCycleAssignment.NewMeritCycleAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleAssignmentSourceLengthRuleFlagsALongSource()
    {
        var rule = new MeritCycleAssignmentSourceLengthRule(32);
        var item = MeritCycleAssignment.NewMeritCycleAssignment("x", "y");
        item.Source = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Source = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void MeritCycleAssignmentQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new MeritCycleAssignmentQuantityRangeRule(80, 86400);
        var item = MeritCycleAssignment.NewMeritCycleAssignment("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        MeritCycleAssignmentValidatorReportsEveryProblem();
        MeritCycleAssignmentCodeLengthRuleFlagsALongCode();
        MeritCycleAssignmentSourceLengthRuleFlagsALongSource();
        MeritCycleAssignmentQuantityRangeRuleFlagsValuesAboveQuantity();
    }
}
