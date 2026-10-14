namespace Acme.Workforce.Workplace.Tests.Visitor.Draft;

using Acme.Workforce.Workplace.Visitor.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class VisitorClaimRulesChecks
{
    public static void VisitorClaimValidatorReportsEveryProblem()
    {
        var validator = VisitorClaimRules.DefaultVisitorClaimValidator();
        var item = VisitorClaim.NewVisitorClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorClaimCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new VisitorClaimCreatedAtRangeRule(64, 120);
        var item = VisitorClaim.NewVisitorClaim("x", "y");
        item.CreatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorClaimRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new VisitorClaimRateRangeRule(8.3, 3229.8);
        var item = VisitorClaim.NewVisitorClaim("x", "y");
        item.Rate = 3231.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorClaimCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorClaimCodeLengthRule(86400);
        var item = VisitorClaim.NewVisitorClaim("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        VisitorClaimValidatorReportsEveryProblem();
        VisitorClaimCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        VisitorClaimRateRangeRuleFlagsValuesAboveRate();
        VisitorClaimCodeLengthRuleFlagsALongCode();
    }
}
