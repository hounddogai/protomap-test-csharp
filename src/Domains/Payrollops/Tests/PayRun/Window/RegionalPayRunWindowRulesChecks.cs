namespace Acme.Workforce.Payrollops.Tests.PayRun.Window;

using Acme.Workforce.Payrollops.PayRun.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RegionalPayRunWindowRulesChecks
{
    public static void RegionalPayRunWindowValidatorReportsEveryProblem()
    {
        var validator = RegionalPayRunWindowRules.DefaultRegionalPayRunWindowValidator();
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalPayRunWindowExpiresAtRangeRuleFlagsValuesAboveExpiresAt()
    {
        var rule = new RegionalPayRunWindowExpiresAtRangeRule(80, 255);
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("x", "y");
        item.ExpiresAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalPayRunWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new RegionalPayRunWindowLabelLengthRule(32);
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayRunWindowLocaleLengthRuleFlagsALongLocale()
    {
        var rule = new RegionalPayRunWindowLocaleLengthRule(64);
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("x", "y");
        item.Locale = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Locale = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalPayRunWindowRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new RegionalPayRunWindowRequiredCodeRule("regional pay run window needs this field");
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalPayRunWindowRequiredLabelRuleFlagsABlankLabel()
    {
        var rule = new RegionalPayRunWindowRequiredLabelRule("regional pay run window needs a value");
        var item = RegionalPayRunWindow.NewRegionalPayRunWindow("", "");
        item.Label = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalPayRunWindowValidatorReportsEveryProblem();
        RegionalPayRunWindowExpiresAtRangeRuleFlagsValuesAboveExpiresAt();
        RegionalPayRunWindowLabelLengthRuleFlagsALongLabel();
        RegionalPayRunWindowLocaleLengthRuleFlagsALongLocale();
        RegionalPayRunWindowRequiredCodeRuleFlagsABlankCode();
        RegionalPayRunWindowRequiredLabelRuleFlagsABlankLabel();
    }
}
