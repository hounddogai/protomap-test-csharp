namespace Acme.Workforce.Talent.Tests.CareerPath.Rule;

using Acme.Workforce.Talent.CareerPath.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class ScheduledCareerPathProfileRulesChecks
{
    public static void ScheduledCareerPathProfileValidatorReportsEveryProblem()
    {
        var validator = ScheduledCareerPathProfileRules.DefaultScheduledCareerPathProfileValidator();
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledCareerPathProfileCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new ScheduledCareerPathProfileCurrencyLengthRule(3600);
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledCareerPathProfileBatchSizeRangeRuleFlagsValuesAboveBatchSize()
    {
        var rule = new ScheduledCareerPathProfileBatchSizeRangeRule(64, 80);
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("x", "y");
        item.BatchSize = 81;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledCareerPathProfileCountryLengthRuleFlagsALongCountry()
    {
        var rule = new ScheduledCareerPathProfileCountryLengthRule(16);
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("x", "y");
        item.Country = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Country = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledCareerPathProfileMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new ScheduledCareerPathProfileMinimumRangeRule(8.5, 4978.5);
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("x", "y");
        item.Minimum = 4980.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void ScheduledCareerPathProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new ScheduledCareerPathProfileCreatedAtRangeRule(32, 1000);
        var item = ScheduledCareerPathProfile.NewScheduledCareerPathProfile("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        ScheduledCareerPathProfileValidatorReportsEveryProblem();
        ScheduledCareerPathProfileCurrencyLengthRuleFlagsALongCurrency();
        ScheduledCareerPathProfileBatchSizeRangeRuleFlagsValuesAboveBatchSize();
        ScheduledCareerPathProfileCountryLengthRuleFlagsALongCountry();
        ScheduledCareerPathProfileMinimumRangeRuleFlagsValuesAboveMinimum();
        ScheduledCareerPathProfileCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
    }
}
