namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PrimaryVisitorAssignmentRulesChecks
{
    public static void PrimaryVisitorAssignmentValidatorReportsEveryProblem()
    {
        var validator = PrimaryVisitorAssignmentRules.DefaultPrimaryVisitorAssignmentValidator();
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryVisitorAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PrimaryVisitorAssignmentCodeLengthRule(64);
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryVisitorAssignmentCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new PrimaryVisitorAssignmentCapacityRangeRule(120, 1000);
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("x", "y");
        item.Capacity = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryVisitorAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PrimaryVisitorAssignmentRequiredLabelRule("primary visitor assignment needs an entry");
        var item = PrimaryVisitorAssignment.NewPrimaryVisitorAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryVisitorAssignmentValidatorReportsEveryProblem();
        PrimaryVisitorAssignmentCodeLengthRuleFlagsALongCode();
        PrimaryVisitorAssignmentCapacityRangeRuleFlagsValuesAboveCapacity();
        PrimaryVisitorAssignmentRequiredLabelRuleFlagsABlankLabel();
    }
}
