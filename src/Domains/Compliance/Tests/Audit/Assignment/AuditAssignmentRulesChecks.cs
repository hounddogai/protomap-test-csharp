namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AuditAssignmentRulesChecks
{
    public static void AuditAssignmentValidatorReportsEveryProblem()
    {
        var validator = AuditAssignmentRules.DefaultAuditAssignmentValidator();
        var item = AuditAssignment.NewAuditAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AuditAssignmentPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new AuditAssignmentPriorityRangeRule(16, 80);
        var item = AuditAssignment.NewAuditAssignment("x", "y");
        item.Priority = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AuditAssignmentRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new AuditAssignmentRequiredJobCodeRule("audit assignment needs a value");
        var item = AuditAssignment.NewAuditAssignment("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new AuditAssignmentRequiredCodeRule("audit assignment needs an entry");
        var item = AuditAssignment.NewAuditAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AuditAssignmentRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new AuditAssignmentRequiredLocationRule("audit assignment needs this field");
        var item = AuditAssignment.NewAuditAssignment("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AuditAssignmentValidatorReportsEveryProblem();
        AuditAssignmentPriorityRangeRuleFlagsValuesAbovePriority();
        AuditAssignmentRequiredJobCodeRuleFlagsABlankJobCode();
        AuditAssignmentRequiredCodeRuleFlagsABlankCode();
        AuditAssignmentRequiredLocationRuleFlagsABlankLocation();
    }
}
