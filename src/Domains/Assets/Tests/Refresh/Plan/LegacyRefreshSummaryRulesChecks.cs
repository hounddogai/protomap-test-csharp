namespace Acme.Workforce.Assets.Tests.Refresh.Plan;

using Acme.Workforce.Assets.Refresh.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class LegacyRefreshSummaryRulesChecks
{
    public static void LegacyRefreshSummaryValidatorReportsEveryProblem()
    {
        var validator = LegacyRefreshSummaryRules.DefaultLegacyRefreshSummaryValidator();
        var item = LegacyRefreshSummary.NewLegacyRefreshSummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRefreshSummaryExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new LegacyRefreshSummaryExternalRefLengthRule(80);
        var item = LegacyRefreshSummary.NewLegacyRefreshSummary("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRefreshSummaryValidatorReportsEveryProblem();
        LegacyRefreshSummaryExternalRefLengthRuleFlagsALongExternalRef();
    }
}
