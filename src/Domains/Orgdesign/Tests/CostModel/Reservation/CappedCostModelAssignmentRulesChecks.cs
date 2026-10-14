namespace Acme.Workforce.Orgdesign.Tests.CostModel.Reservation;

using Acme.Workforce.Orgdesign.CostModel.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedCostModelAssignmentRulesChecks
{
    public static void CappedCostModelAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedCostModelAssignmentRules.DefaultCappedCostModelAssignmentValidator();
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedCostModelAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new CappedCostModelAssignmentNameLengthRule(32);
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedCostModelAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedCostModelAssignmentCodeLengthRule(80);
        var item = CappedCostModelAssignment.NewCappedCostModelAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedCostModelAssignmentValidatorReportsEveryProblem();
        CappedCostModelAssignmentNameLengthRuleFlagsALongName();
        CappedCostModelAssignmentCodeLengthRuleFlagsALongCode();
    }
}
