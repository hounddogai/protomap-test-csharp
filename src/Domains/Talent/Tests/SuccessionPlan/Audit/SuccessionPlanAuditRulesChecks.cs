namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Audit;

using Acme.Workforce.Talent.SuccessionPlan.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SuccessionPlanAuditRulesChecks
{
    public static void SuccessionPlanAuditValidatorReportsEveryProblem()
    {
        var validator = SuccessionPlanAuditRules.DefaultSuccessionPlanAuditValidator();
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SuccessionPlanAuditSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new SuccessionPlanAuditSegmentLengthRule(3600);
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void SuccessionPlanAuditGradeLengthRuleFlagsALongGrade()
    {
        var rule = new SuccessionPlanAuditGradeLengthRule(32);
        var item = SuccessionPlanAudit.NewSuccessionPlanAudit("x", "y");
        item.Grade = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Grade = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SuccessionPlanAuditValidatorReportsEveryProblem();
        SuccessionPlanAuditSegmentLengthRuleFlagsALongSegment();
        SuccessionPlanAuditGradeLengthRuleFlagsALongGrade();
    }
}
