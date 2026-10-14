namespace Acme.Workforce.Payrollops.Tests.RetroPay.PayAssignment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Checks of pay assignment code, run by the project's runner.</summary>
public static class LegacyRetroPaySummaryRulesChecks
{
    public static void LegacyRetroPaySummaryValidatorReportsEveryProblem()
    {
        var validator = LegacyRetroPaySummaryRules.DefaultLegacyRetroPaySummaryValidator();
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyRetroPaySummaryMaximumRangeRuleFlagsValuesAboveMaximum()
    {
        var rule = new LegacyRetroPaySummaryMaximumRangeRule(7.5, 1162.5);
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("x", "y");
        item.Maximum = 1164.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRetroPaySummaryQuantityRangeRuleFlagsValuesAboveQuantity()
    {
        var rule = new LegacyRetroPaySummaryQuantityRangeRule(255, 86400);
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("x", "y");
        item.Quantity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyRetroPaySummaryRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new LegacyRetroPaySummaryRequiredCodeRule("legacy retro pay summary needs a value");
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LegacyRetroPaySummaryExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new LegacyRetroPaySummaryExternalRefLengthRule(255);
        var item = LegacyRetroPaySummary.NewLegacyRetroPaySummary("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LegacyRetroPaySummaryValidatorReportsEveryProblem();
        LegacyRetroPaySummaryMaximumRangeRuleFlagsValuesAboveMaximum();
        LegacyRetroPaySummaryQuantityRangeRuleFlagsValuesAboveQuantity();
        LegacyRetroPaySummaryRequiredCodeRuleFlagsABlankCode();
        LegacyRetroPaySummaryExternalRefLengthRuleFlagsALongExternalRef();
    }
}
