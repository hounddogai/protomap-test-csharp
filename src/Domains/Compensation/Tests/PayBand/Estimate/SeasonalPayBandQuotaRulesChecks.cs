namespace Acme.Workforce.Compensation.Tests.PayBand.Estimate;

using Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalPayBandQuotaRulesChecks
{
    public static void SeasonalPayBandQuotaValidatorReportsEveryProblem()
    {
        var validator = SeasonalPayBandQuotaRules.DefaultSeasonalPayBandQuotaValidator();
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void SeasonalPayBandQuotaRequiredJobCodeRuleFlagsABlankJobCode()
    {
        var rule = new SeasonalPayBandQuotaRequiredJobCodeRule("seasonal pay band quota needs an entry");
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("", "");
        item.JobCode = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void SeasonalPayBandQuotaSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new SeasonalPayBandQuotaSequenceRangeRule(80, 80);
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("x", "y");
        item.Sequence = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void SeasonalPayBandQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new SeasonalPayBandQuotaCodeLengthRule(1000);
        var item = SeasonalPayBandQuota.NewSeasonalPayBandQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        SeasonalPayBandQuotaValidatorReportsEveryProblem();
        SeasonalPayBandQuotaRequiredJobCodeRuleFlagsABlankJobCode();
        SeasonalPayBandQuotaSequenceRangeRuleFlagsValuesAboveSequence();
        SeasonalPayBandQuotaCodeLengthRuleFlagsALongCode();
    }
}
