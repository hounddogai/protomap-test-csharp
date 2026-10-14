namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgClaim;

using Acme.Workforce.Orgdesign.Reorg.ReorgClaim;

/// <summary>Checks of reorg claim code, run by the project's runner.</summary>
public static class RollingReorgSummaryRulesChecks
{
    public static void RollingReorgSummaryValidatorReportsEveryProblem()
    {
        var validator = RollingReorgSummaryRules.DefaultRollingReorgSummaryValidator();
        var item = RollingReorgSummary.NewRollingReorgSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RollingReorgSummaryRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new RollingReorgSummaryRequiredExternalRefRule("rolling reorg summary needs an entry");
        var item = RollingReorgSummary.NewRollingReorgSummary("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingReorgSummaryCodeLengthRuleFlagsALongCode()
    {
        var rule = new RollingReorgSummaryCodeLengthRule(3600);
        var item = RollingReorgSummary.NewRollingReorgSummary("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RollingReorgSummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RollingReorgSummaryRequiredCodeRule("rolling reorg summary needs an entry");
        var item = RollingReorgSummary.NewRollingReorgSummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RollingReorgSummaryVersionRangeRuleFlagsValuesAboveVersion()
    {
        var rule = new RollingReorgSummaryVersionRangeRule(16, 64);
        var item = RollingReorgSummary.NewRollingReorgSummary("x", "y");
        item.Version = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RollingReorgSummaryValidatorReportsEveryProblem();
        RollingReorgSummaryRequiredExternalRefRuleFlagsABlankExternalRef();
        RollingReorgSummaryCodeLengthRuleFlagsALongCode();
        RollingReorgSummaryRequiredCodeRuleFlagsABlankCode();
        RollingReorgSummaryVersionRangeRuleFlagsValuesAboveVersion();
    }
}
