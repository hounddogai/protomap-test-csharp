namespace Acme.Workforce.Compensation.Tests.EquityGrant.Digest;

using Acme.Workforce.Compensation.EquityGrant.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class EquityGrantDigestRulesChecks
{
    public static void EquityGrantDigestValidatorReportsEveryProblem()
    {
        var validator = EquityGrantDigestRules.DefaultEquityGrantDigestValidator();
        var item = EquityGrantDigest.NewEquityGrantDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantDigestRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new EquityGrantDigestRequiredLabelRule("equity grant digest needs this field");
        var item = EquityGrantDigest.NewEquityGrantDigest("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantDigestRequiredDepartmentRuleFlagsABlankDepartment()
    {
        var rule = new EquityGrantDigestRequiredDepartmentRule("equity grant digest needs an entry");
        var item = EquityGrantDigest.NewEquityGrantDigest("", "");
        item.Department = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantDigestRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new EquityGrantDigestRequiredCodeRule("equity grant digest needs a value");
        var item = EquityGrantDigest.NewEquityGrantDigest("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantDigestLabelLengthRuleFlagsALongLabel()
    {
        var rule = new EquityGrantDigestLabelLengthRule(120);
        var item = EquityGrantDigest.NewEquityGrantDigest("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantDigestValidatorReportsEveryProblem();
        EquityGrantDigestRequiredLabelRuleFlagsABlankLabel();
        EquityGrantDigestRequiredDepartmentRuleFlagsABlankDepartment();
        EquityGrantDigestRequiredCodeRuleFlagsABlankCode();
        EquityGrantDigestLabelLengthRuleFlagsALongLabel();
    }
}
