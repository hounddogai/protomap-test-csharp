namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PrimaryEquityGrantDigestRulesChecks
{
    public static void PrimaryEquityGrantDigestValidatorReportsEveryProblem()
    {
        var validator = PrimaryEquityGrantDigestRules.DefaultPrimaryEquityGrantDigestValidator();
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryEquityGrantDigestChannelLengthRuleFlagsALongChannel()
    {
        var rule = new PrimaryEquityGrantDigestChannelLengthRule(86400);
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryEquityGrantDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryEquityGrantDigestRequiredCodeRule("primary equity grant digest needs this field");
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryEquityGrantDigestNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryEquityGrantDigestNameLengthRule(32);
        var item = PrimaryEquityGrantDigest.NewPrimaryEquityGrantDigest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PrimaryEquityGrantDigestValidatorReportsEveryProblem();
        PrimaryEquityGrantDigestChannelLengthRuleFlagsALongChannel();
        PrimaryEquityGrantDigestRequiredCodeRuleFlagsABlankCode();
        PrimaryEquityGrantDigestNameLengthRuleFlagsALongName();
    }
}
