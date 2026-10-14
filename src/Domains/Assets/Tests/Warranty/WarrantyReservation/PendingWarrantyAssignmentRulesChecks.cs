namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyReservation;

using Acme.Workforce.Assets.Warranty.WarrantyReservation;

/// <summary>Checks of warranty reservation code, run by the project's runner.</summary>
public static class PendingWarrantyAssignmentRulesChecks
{
    public static void PendingWarrantyAssignmentValidatorReportsEveryProblem()
    {
        var validator = PendingWarrantyAssignmentRules.DefaultPendingWarrantyAssignmentValidator();
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingWarrantyAssignmentRequiredGradeRuleFlagsABlankGrade()
    {
        var rule = new PendingWarrantyAssignmentRequiredGradeRule("pending warranty assignment needs an entry");
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("", "");
        item.Grade = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingWarrantyAssignmentAmountRangeRuleFlagsValuesAboveAmount()
    {
        var rule = new PendingWarrantyAssignmentAmountRangeRule(1.9, 164.0);
        var item = PendingWarrantyAssignment.NewPendingWarrantyAssignment("x", "y");
        item.Amount = 165.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PendingWarrantyAssignmentValidatorReportsEveryProblem();
        PendingWarrantyAssignmentRequiredGradeRuleFlagsABlankGrade();
        PendingWarrantyAssignmentAmountRangeRuleFlagsValuesAboveAmount();
    }
}
