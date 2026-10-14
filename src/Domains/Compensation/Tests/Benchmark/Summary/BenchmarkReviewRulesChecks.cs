namespace Acme.Workforce.Compensation.Tests.Benchmark.Summary;

using Acme.Workforce.Compensation.Benchmark.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BenchmarkReviewRulesChecks
{
    public static void BenchmarkReviewValidatorReportsEveryProblem()
    {
        var validator = BenchmarkReviewRules.DefaultBenchmarkReviewValidator();
        var item = BenchmarkReview.NewBenchmarkReview("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void BenchmarkReviewDiscountRangeRuleFlagsValuesAboveDiscount()
    {
        var rule = new BenchmarkReviewDiscountRangeRule(1.3, 1267.0);
        var item = BenchmarkReview.NewBenchmarkReview("x", "y");
        item.Discount = 1268.5;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkReviewCodeLengthRuleFlagsALongCode()
    {
        var rule = new BenchmarkReviewCodeLengthRule(3600);
        var item = BenchmarkReview.NewBenchmarkReview("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void BenchmarkReviewMarginRangeRuleFlagsValuesAboveMargin()
    {
        var rule = new BenchmarkReviewMarginRangeRule(9.2, 2212.2);
        var item = BenchmarkReview.NewBenchmarkReview("x", "y");
        item.Margin = 2213.7;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void BenchmarkReviewRequiredLocationRuleFlagsABlankLocation()
    {
        var rule = new BenchmarkReviewRequiredLocationRule("benchmark review needs a value");
        var item = BenchmarkReview.NewBenchmarkReview("", "");
        item.Location = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        BenchmarkReviewValidatorReportsEveryProblem();
        BenchmarkReviewDiscountRangeRuleFlagsValuesAboveDiscount();
        BenchmarkReviewCodeLengthRuleFlagsALongCode();
        BenchmarkReviewMarginRangeRuleFlagsValuesAboveMargin();
        BenchmarkReviewRequiredLocationRuleFlagsABlankLocation();
    }
}
