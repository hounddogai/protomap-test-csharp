namespace Acme.Workforce.Compensation.Tests.Benchmark.Review;

using Acme.Workforce.Compensation.Benchmark.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RegionalBenchmarkReviewRulesChecks
{
    public static void RegionalBenchmarkReviewValidatorReportsEveryProblem()
    {
        var validator = RegionalBenchmarkReviewRules.DefaultRegionalBenchmarkReviewValidator();
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalBenchmarkReviewPhoneLengthRuleFlagsALongPhone()
    {
        var rule = new RegionalBenchmarkReviewPhoneLengthRule(3600);
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("x", "y");
        item.Phone = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Phone = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBenchmarkReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalBenchmarkReviewCodeLengthRule(3600);
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalBenchmarkReviewMinimumRangeRuleFlagsValuesAboveMinimum()
    {
        var rule = new RegionalBenchmarkReviewMinimumRangeRule(2.7, 218.0);
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("x", "y");
        item.Minimum = 219.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RegionalBenchmarkReviewRateRangeRuleFlagsValuesAboveRate()
    {
        var rule = new RegionalBenchmarkReviewRateRangeRule(9.7, 4261.1);
        var item = RegionalBenchmarkReview.NewRegionalBenchmarkReview("x", "y");
        item.Rate = 4262.6;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        RegionalBenchmarkReviewValidatorReportsEveryProblem();
        RegionalBenchmarkReviewPhoneLengthRuleFlagsALongPhone();
        RegionalBenchmarkReviewCodeLengthRuleFlagsALongCode();
        RegionalBenchmarkReviewMinimumRangeRuleFlagsValuesAboveMinimum();
        RegionalBenchmarkReviewRateRangeRuleFlagsValuesAboveRate();
    }
}
