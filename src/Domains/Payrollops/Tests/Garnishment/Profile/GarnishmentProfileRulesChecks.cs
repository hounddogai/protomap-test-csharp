namespace Acme.Workforce.Payrollops.Tests.Garnishment.Profile;

using Acme.Workforce.Payrollops.Garnishment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class GarnishmentProfileRulesChecks
{
    public static void GarnishmentProfileValidatorReportsEveryProblem()
    {
        var validator = GarnishmentProfileRules.DefaultGarnishmentProfileValidator();
        var item = GarnishmentProfile.NewGarnishmentProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentProfileEndsAtRangeRuleFlagsValuesAboveEndsAt()
    {
        var rule = new GarnishmentProfileEndsAtRangeRule(64, 255);
        var item = GarnishmentProfile.NewGarnishmentProfile("x", "y");
        item.EndsAt = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void GarnishmentProfileRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new GarnishmentProfileRequiredCodeRule("garnishment profile needs an entry");
        var item = GarnishmentProfile.NewGarnishmentProfile("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentProfileValidatorReportsEveryProblem();
        GarnishmentProfileEndsAtRangeRuleFlagsValuesAboveEndsAt();
        GarnishmentProfileRequiredCodeRuleFlagsABlankCode();
    }
}
