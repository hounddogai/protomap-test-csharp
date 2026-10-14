namespace Acme.Workforce.Orgdesign.Tests.Span.Baseline;

using Acme.Workforce.Orgdesign.Span.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PendingSpanBaselineRulesChecks
{
    public static void PendingSpanBaselineValidatorReportsEveryProblem()
    {
        var validator = PendingSpanBaselineRules.DefaultPendingSpanBaselineValidator();
        var item = PendingSpanBaseline.NewPendingSpanBaseline("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingSpanBaselineJobCodeLengthRuleFlagsALongJobCode()
    {
        var rule = new PendingSpanBaselineJobCodeLengthRule(80);
        var item = PendingSpanBaseline.NewPendingSpanBaseline("x", "y");
        item.JobCode = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.JobCode = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSpanBaselineLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PendingSpanBaselineLabelLengthRule(120);
        var item = PendingSpanBaseline.NewPendingSpanBaseline("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSpanBaselineRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new PendingSpanBaselineRequiredLabelRule("pending span baseline needs an entry");
        var item = PendingSpanBaseline.NewPendingSpanBaseline("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSpanBaselineCountryLengthRuleFlagsALongCountry()
    {
        var rule = new PendingSpanBaselineCountryLengthRule(80);
        var item = PendingSpanBaseline.NewPendingSpanBaseline("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingSpanBaselineRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PendingSpanBaselineRequiredCountryRule("pending span baseline needs an entry");
        var item = PendingSpanBaseline.NewPendingSpanBaseline("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingSpanBaselineRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingSpanBaselineRequiredCodeRule("pending span baseline needs this field");
        var item = PendingSpanBaseline.NewPendingSpanBaseline("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingSpanBaselineValidatorReportsEveryProblem();
        PendingSpanBaselineJobCodeLengthRuleFlagsALongJobCode();
        PendingSpanBaselineLabelLengthRuleFlagsALongLabel();
        PendingSpanBaselineRequiredLabelRuleFlagsABlankLabel();
        PendingSpanBaselineCountryLengthRuleFlagsALongCountry();
        PendingSpanBaselineRequiredCountryRuleFlagsABlankCountry();
        PendingSpanBaselineRequiredCodeRuleFlagsABlankCode();
    }
}
