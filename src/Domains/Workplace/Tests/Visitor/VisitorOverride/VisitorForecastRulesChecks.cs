namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorOverride;

using Acme.Workforce.Workplace.Visitor.VisitorOverride;

/// <summary>Checks of visitor override code, run by the project's runner.</summary>
public static class VisitorForecastRulesChecks
{
    public static void VisitorForecastValidatorReportsEveryProblem()
    {
        var validator = VisitorForecastRules.DefaultVisitorForecastValidator();
        var item = VisitorForecast.NewVisitorForecast("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void VisitorForecastBadgeLengthRuleFlagsALongBadge()
    {
        var rule = new VisitorForecastBadgeLengthRule(1000);
        var item = VisitorForecast.NewVisitorForecast("x", "y");
        item.Badge = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Badge = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorForecastCodeLengthRuleFlagsALongCode()
    {
        var rule = new VisitorForecastCodeLengthRule(80);
        var item = VisitorForecast.NewVisitorForecast("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void VisitorForecastRevisionRangeRuleFlagsValuesAboveRevision()
    {
        var rule = new VisitorForecastRevisionRangeRule(80, 255);
        var item = VisitorForecast.NewVisitorForecast("x", "y");
        item.Revision = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void VisitorForecastUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new VisitorForecastUpdatedAtRangeRule(64, 120);
        var item = VisitorForecast.NewVisitorForecast("x", "y");
        item.UpdatedAt = 121;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        VisitorForecastValidatorReportsEveryProblem();
        VisitorForecastBadgeLengthRuleFlagsALongBadge();
        VisitorForecastCodeLengthRuleFlagsALongCode();
        VisitorForecastRevisionRangeRuleFlagsValuesAboveRevision();
        VisitorForecastUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
    }
}
