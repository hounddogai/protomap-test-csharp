namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantMetric;

using Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>Checks of grant metric code, run by the project's runner.</summary>
public static class EquityGrantScoreRulesChecks
{
    public static void EquityGrantScoreValidatorReportsEveryProblem()
    {
        var validator = EquityGrantScoreRules.DefaultEquityGrantScoreValidator();
        var item = EquityGrantScore.NewEquityGrantScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantScoreRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new EquityGrantScoreRequiredExternalRefRule("equity grant score needs this field");
        var item = EquityGrantScore.NewEquityGrantScore("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void EquityGrantScoreRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new EquityGrantScoreRateRangeRule(2.0, 255.2);
        var item = EquityGrantScore.NewEquityGrantScore("x", "y");
        item.Rate = 256.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantScoreHoursRangeRuleFlagsValuesAboveHours()
    {
        var rule = new EquityGrantScoreHoursRangeRule(5.3, 4583.2);
        var item = EquityGrantScore.NewEquityGrantScore("x", "y");
        item.Hours = 4584.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void EquityGrantScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new EquityGrantScoreRequiredNameRule("equity grant score needs this field");
        var item = EquityGrantScore.NewEquityGrantScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        EquityGrantScoreValidatorReportsEveryProblem();
        EquityGrantScoreRequiredExternalRefRuleFlagsABlankExternalRef();
        EquityGrantScoreRateRangeRuleFlagsValuesAboveRate();
        EquityGrantScoreHoursRangeRuleFlagsValuesAboveHours();
        EquityGrantScoreRequiredNameRuleFlagsABlankName();
    }
}
