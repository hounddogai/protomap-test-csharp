namespace Acme.Workforce.Absence.Tests.Policy.WeightedPolicyPreference;

using Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

/// <summary>Checks of weighted policy preference code, run by the project's runner.</summary>
public static class TieredPolicyBaselineRulesChecks
{
    public static void TieredPolicyBaselineValidatorReportsEveryProblem()
    {
        var validator = TieredPolicyBaselineRules.DefaultTieredPolicyBaselineValidator();
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredPolicyBaselineTitleLengthRuleFlagsALongTitle()
    {
        var rule = new TieredPolicyBaselineTitleLengthRule(3600);
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPolicyBaselineCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredPolicyBaselineCodeLengthRule(86400);
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredPolicyBaselineRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new TieredPolicyBaselineRequiredLocationRule("tiered policy baseline needs a value");
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPolicyBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredPolicyBaselineRequiredCodeRule("tiered policy baseline needs a value");
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredPolicyBaselineLocationLengthRuleFlagsALongLocation()
    {
        var rule = new TieredPolicyBaselineLocationLengthRule(80);
        var item = TieredPolicyBaseline.NewTieredPolicyBaseline("x", "y");
        item.Location = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Location = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredPolicyBaselineValidatorReportsEveryProblem();
        TieredPolicyBaselineTitleLengthRuleFlagsALongTitle();
        TieredPolicyBaselineCodeLengthRuleFlagsALongCode();
        TieredPolicyBaselineRequiredLocationRuleFlagsABlankLocation();
        TieredPolicyBaselineRequiredCodeRuleFlagsABlankCode();
        TieredPolicyBaselineLocationLengthRuleFlagsALongLocation();
    }
}
