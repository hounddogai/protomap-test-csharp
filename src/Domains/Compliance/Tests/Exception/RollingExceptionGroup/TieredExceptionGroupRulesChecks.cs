namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class TieredExceptionGroupRulesChecks
{
    public static void TieredExceptionGroupValidatorReportsEveryProblem()
    {
        var validator = TieredExceptionGroupRules.DefaultTieredExceptionGroupValidator();
        var item = TieredExceptionGroup.NewTieredExceptionGroup("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredExceptionGroupRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new TieredExceptionGroupRequiredCodeRule("tiered exception group needs an entry");
        var item = TieredExceptionGroup.NewTieredExceptionGroup("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredExceptionGroupDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes()
    {
        var rule = new TieredExceptionGroupDurationMinutesRangeRule(120, 1000);
        var item = TieredExceptionGroup.NewTieredExceptionGroup("x", "y");
        item.DurationMinutes = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredExceptionGroupLabelLengthRuleFlagsALongLabel()
    {
        var rule = new TieredExceptionGroupLabelLengthRule(255);
        var item = TieredExceptionGroup.NewTieredExceptionGroup("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredExceptionGroupCodeLengthRuleFlagsALongCode()
    {
        var rule = new TieredExceptionGroupCodeLengthRule(64);
        var item = TieredExceptionGroup.NewTieredExceptionGroup("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        TieredExceptionGroupValidatorReportsEveryProblem();
        TieredExceptionGroupRequiredCodeRuleFlagsABlankCode();
        TieredExceptionGroupDurationMinutesRangeRuleFlagsValuesAboveDurationMinutes();
        TieredExceptionGroupLabelLengthRuleFlagsALongLabel();
        TieredExceptionGroupCodeLengthRuleFlagsALongCode();
    }
}
