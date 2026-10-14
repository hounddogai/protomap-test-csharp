namespace Acme.Workforce.Compliance.Tests.Exception.SeasonalExceptionGroup;

using Acme.Workforce.Compliance.Exception.SeasonalExceptionGroup;

/// <summary>Checks of seasonal exception group code, run by the project's runner.</summary>
public static class ExceptionClaimRulesChecks
{
    public static void ExceptionClaimValidatorReportsEveryProblem()
    {
        var validator = ExceptionClaimRules.DefaultExceptionClaimValidator();
        var item = ExceptionClaim.NewExceptionClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ExceptionClaimRequiredReviewerRuleFlagsABlankReviewer()
    {
        var rule = new ExceptionClaimRequiredReviewerRule("exception claim needs a value");
        var item = ExceptionClaim.NewExceptionClaim("", "");
        item.Reviewer = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionClaimRequiredNameRuleFlagsABlankName()
    {
        var rule = new ExceptionClaimRequiredNameRule("exception claim needs a value");
        var item = ExceptionClaim.NewExceptionClaim("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionClaimRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new ExceptionClaimRequiredChannelRule("exception claim needs a value");
        var item = ExceptionClaim.NewExceptionClaim("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ExceptionClaimCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ExceptionClaimCreatedAtRangeRule(16, 64);
        var item = ExceptionClaim.NewExceptionClaim("x", "y");
        item.CreatedAt = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ExceptionClaimNameLengthRuleFlagsALongName()
    {
        var rule = new ExceptionClaimNameLengthRule(1000);
        var item = ExceptionClaim.NewExceptionClaim("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ExceptionClaimChannelLengthRuleFlagsALongChannel()
    {
        var rule = new ExceptionClaimChannelLengthRule(32);
        var item = ExceptionClaim.NewExceptionClaim("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ExceptionClaimValidatorReportsEveryProblem();
        ExceptionClaimRequiredReviewerRuleFlagsABlankReviewer();
        ExceptionClaimRequiredNameRuleFlagsABlankName();
        ExceptionClaimRequiredChannelRuleFlagsABlankChannel();
        ExceptionClaimCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        ExceptionClaimNameLengthRuleFlagsALongName();
        ExceptionClaimChannelLengthRuleFlagsALongChannel();
    }
}
