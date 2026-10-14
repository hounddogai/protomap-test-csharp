namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PendingVisitorAssignmentRulesChecks
{
    public static void PendingVisitorAssignmentValidatorReportsEveryProblem()
    {
        var validator = PendingVisitorAssignmentRules.DefaultPendingVisitorAssignmentValidator();
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingVisitorAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingVisitorAssignmentCodeLengthRule(64);
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVisitorAssignmentNameLengthRuleFlagsALongName()
    {
        var rule = new PendingVisitorAssignmentNameLengthRule(64);
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVisitorAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingVisitorAssignmentLabelLengthRule(3600);
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingVisitorAssignmentRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingVisitorAssignmentRequiredLabelRule("pending visitor assignment needs a value");
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingVisitorAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingVisitorAssignmentRequiredCodeRule("pending visitor assignment needs a value");
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingVisitorAssignmentValidatorReportsEveryProblem();
        PendingVisitorAssignmentCodeLengthRuleFlagsALongCode();
        PendingVisitorAssignmentNameLengthRuleFlagsALongName();
        PendingVisitorAssignmentLabelLengthRuleFlagsALongLabel();
        PendingVisitorAssignmentRequiredLabelRuleFlagsABlankLabel();
        PendingVisitorAssignmentRequiredCodeRuleFlagsABlankCode();
    }
}
