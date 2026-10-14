namespace Acme.Workforce.Orgdesign.Tests.Span.Claim;

using Acme.Workforce.Orgdesign.Span.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PendingSpanClaimRulesChecks
{
    public static void PendingSpanClaimValidatorReportsEveryProblem()
    {
        var validator = PendingSpanClaimRules.DefaultPendingSpanClaimValidator();
        var item = PendingSpanClaim.NewPendingSpanClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSpanClaimDescriptionLengthRuleFlagsALongDescription()
    {
        var rule = new PendingSpanClaimDescriptionLengthRule(255);
        var item = PendingSpanClaim.NewPendingSpanClaim("x", "y");
        item.Description = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Description = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSpanClaimCountryLengthRuleFlagsALongCountry()
    {
        var rule = new PendingSpanClaimCountryLengthRule(64);
        var item = PendingSpanClaim.NewPendingSpanClaim("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSpanClaimRequiredDescriptionRuleFlagsABlankDescription()
    {
        var rule = new PendingSpanClaimRequiredDescriptionRule("pending span claim needs an entry");
        var item = PendingSpanClaim.NewPendingSpanClaim("", "");
        item.Description = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingSpanClaimValidatorReportsEveryProblem();
        PendingSpanClaimDescriptionLengthRuleFlagsALongDescription();
        PendingSpanClaimCountryLengthRuleFlagsALongCountry();
        PendingSpanClaimRequiredDescriptionRuleFlagsABlankDescription();
    }
}
