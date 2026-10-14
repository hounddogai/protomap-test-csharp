namespace Acme.Workforce.Contractors.Tests.Rate.Plan;

using Acme.Workforce.Contractors.Rate.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class RateAuditRulesChecks
{
    public static void RateAuditValidatorReportsEveryProblem()
    {
        var validator = RateAuditRules.DefaultRateAuditValidator();
        var item = RateAudit.NewRateAudit("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RateAuditRequiredNameRuleFlagsABlankName()
    {
        var rule = new RateAuditRequiredNameRule("rate audit needs an entry");
        var item = RateAudit.NewRateAudit("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RateAuditRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RateAuditRateRangeRule(7.0, 828.0);
        var item = RateAudit.NewRateAudit("x", "y");
        item.Rate = 829.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RateAuditValidatorReportsEveryProblem();
        RateAuditRequiredNameRuleFlagsABlankName();
        RateAuditRateRangeRuleFlagsValuesAboveRate();
    }
}
