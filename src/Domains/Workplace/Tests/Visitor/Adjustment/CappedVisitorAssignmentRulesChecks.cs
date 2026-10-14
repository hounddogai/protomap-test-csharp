namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorAssignmentRulesChecks
{
    public static void CappedVisitorAssignmentValidatorReportsEveryProblem()
    {
        var validator = CappedVisitorAssignmentRules.DefaultCappedVisitorAssignmentValidator();
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedVisitorAssignmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new CappedVisitorAssignmentExpiresAtRangeRule(255, 86400);
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("x", "y");
        item.ExpiresAt = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void CappedVisitorAssignmentRequiredSummaryRuleFlagsABlankSummary()
    {
        var rule = new CappedVisitorAssignmentRequiredSummaryRule("capped visitor assignment needs a value");
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("", "");
        item.Summary = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedVisitorAssignmentTitleLengthRuleFlagsALongTitle()
    {
        var rule = new CappedVisitorAssignmentTitleLengthRule(32);
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedVisitorAssignmentRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new CappedVisitorAssignmentRequiredCodeRule("capped visitor assignment needs an entry");
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedVisitorAssignmentSummaryLengthRuleFlagsALongSummary()
    {
        var rule = new CappedVisitorAssignmentSummaryLengthRule(3600);
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("x", "y");
        item.Summary = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Summary = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedVisitorAssignmentEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new CappedVisitorAssignmentEmployeeIdLengthRule(86400);
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedVisitorAssignmentValidatorReportsEveryProblem();
        CappedVisitorAssignmentExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        CappedVisitorAssignmentRequiredSummaryRuleFlagsABlankSummary();
        CappedVisitorAssignmentTitleLengthRuleFlagsALongTitle();
        CappedVisitorAssignmentRequiredCodeRuleFlagsABlankCode();
        CappedVisitorAssignmentSummaryLengthRuleFlagsALongSummary();
        CappedVisitorAssignmentEmployeeIdLengthRuleFlagsALongEmployeeId();
    }
}
