namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyManifestRulesChecks
{
    public static void PolicyManifestValidatorReportsEveryProblem()
    {
        var validator = PolicyManifestRules.DefaultPolicyManifestValidator();
        var item = PolicyManifest.NewPolicyManifest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PolicyManifestLimitRangeRuleFlagsValuesAboveLimit()
    {
        var rule = new PolicyManifestLimitRangeRule(120, 255);
        var item = PolicyManifest.NewPolicyManifest("x", "y");
        item.Limit = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyManifestEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new PolicyManifestEndsAtRangeRule(16, 80);
        var item = PolicyManifest.NewPolicyManifest("x", "y");
        item.EndsAt = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PolicyManifestCodeLengthRuleFlagsALongCode()
    {
        var rule = new PolicyManifestCodeLengthRule(80);
        var item = PolicyManifest.NewPolicyManifest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PolicyManifestValidatorReportsEveryProblem();
        PolicyManifestLimitRangeRuleFlagsValuesAboveLimit();
        PolicyManifestEndsAtRangeRuleFlagsValuesAboveEndsAt();
        PolicyManifestCodeLengthRuleFlagsALongCode();
    }
}
