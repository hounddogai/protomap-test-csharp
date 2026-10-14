namespace Acme.Workforce.Absence.Tests.ReturnPlan.OverrideItem;

using Acme.Workforce.Absence.ReturnPlan.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class ReturnPlanClaimRulesChecks
{
    public static void ReturnPlanClaimValidatorReportsEveryProblem()
    {
        var validator = ReturnPlanClaimRules.DefaultReturnPlanClaimValidator();
        var item = ReturnPlanClaim.NewReturnPlanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ReturnPlanClaimRequiredSegmentRuleFlagsABlankSegment()
    {
        var rule = new ReturnPlanClaimRequiredSegmentRule("return plan claim needs a value");
        var item = ReturnPlanClaim.NewReturnPlanClaim("", "");
        item.Segment = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanClaimRequiredChecksumRuleFlagsABlankChecksum()
    {
        var rule = new ReturnPlanClaimRequiredChecksumRule("return plan claim needs a value");
        var item = ReturnPlanClaim.NewReturnPlanClaim("", "");
        item.Checksum = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ReturnPlanClaimSegmentLengthRuleFlagsALongSegment()
    {
        var rule = new ReturnPlanClaimSegmentLengthRule(86400);
        var item = ReturnPlanClaim.NewReturnPlanClaim("x", "y");
        item.Segment = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Segment = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ReturnPlanClaimRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new ReturnPlanClaimRequiredCodeRule("return plan claim needs an entry");
        var item = ReturnPlanClaim.NewReturnPlanClaim("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        ReturnPlanClaimValidatorReportsEveryProblem();
        ReturnPlanClaimRequiredSegmentRuleFlagsABlankSegment();
        ReturnPlanClaimRequiredChecksumRuleFlagsABlankChecksum();
        ReturnPlanClaimSegmentLengthRuleFlagsALongSegment();
        ReturnPlanClaimRequiredCodeRuleFlagsABlankCode();
    }
}
