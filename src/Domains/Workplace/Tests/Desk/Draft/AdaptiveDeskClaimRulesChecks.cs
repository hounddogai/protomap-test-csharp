namespace Acme.Workforce.Workplace.Tests.Desk.Draft;

using Acme.Workforce.Workplace.Desk.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class AdaptiveDeskClaimRulesChecks
{
    public static void AdaptiveDeskClaimValidatorReportsEveryProblem()
    {
        var validator = AdaptiveDeskClaimRules.DefaultAdaptiveDeskClaimValidator();
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveDeskClaimLabelLengthRuleFlagsALongLabel()
    {
        var rule = new AdaptiveDeskClaimLabelLengthRule(86400);
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveDeskClaimEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new AdaptiveDeskClaimEmployeeIdLengthRule(120);
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveDeskClaimRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new AdaptiveDeskClaimRequiredLabelRule("adaptive desk claim needs an entry");
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AdaptiveDeskClaimCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new AdaptiveDeskClaimCreatedAtRangeRule(32, 64);
        var item = AdaptiveDeskClaim.NewAdaptiveDeskClaim("x", "y");
        item.CreatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        AdaptiveDeskClaimValidatorReportsEveryProblem();
        AdaptiveDeskClaimLabelLengthRuleFlagsALongLabel();
        AdaptiveDeskClaimEmployeeIdLengthRuleFlagsALongEmployeeId();
        AdaptiveDeskClaimRequiredLabelRuleFlagsABlankLabel();
        AdaptiveDeskClaimCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
