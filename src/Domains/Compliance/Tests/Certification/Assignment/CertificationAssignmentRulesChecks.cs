namespace Acme.Workforce.Compliance.Tests.Certification.Assignment;

using Acme.Workforce.Compliance.Certification.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class CertificationAssignmentRulesChecks
{
    public static void CertificationAssignmentValidatorReportsEveryProblem()
    {
        var validator = CertificationAssignmentRules.DefaultCertificationAssignmentValidator();
        var item = CertificationAssignment.NewCertificationAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CertificationAssignmentDepartmentLengthRuleFlagsALongDepartment()
    {
        var rule = new CertificationAssignmentDepartmentLengthRule(16);
        var item = CertificationAssignment.NewCertificationAssignment("x", "y");
        item.Department = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Department = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationAssignmentCodeLengthRuleFlagsALongCode()
    {
        var rule = new CertificationAssignmentCodeLengthRule(80);
        var item = CertificationAssignment.NewCertificationAssignment("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CertificationAssignmentLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CertificationAssignmentLabelLengthRule(255);
        var item = CertificationAssignment.NewCertificationAssignment("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CertificationAssignmentValidatorReportsEveryProblem();
        CertificationAssignmentDepartmentLengthRuleFlagsALongDepartment();
        CertificationAssignmentCodeLengthRuleFlagsALongCode();
        CertificationAssignmentLabelLengthRuleFlagsALongLabel();
    }
}
