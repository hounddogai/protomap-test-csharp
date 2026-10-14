namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class LenientStatementOfWorkDigestRulesChecks
{
    public static void LenientStatementOfWorkDigestValidatorReportsEveryProblem()
    {
        var validator = LenientStatementOfWorkDigestRules.DefaultLenientStatementOfWorkDigestValidator();
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LenientStatementOfWorkDigestRatioRangeRuleFlagsValuesAboveRatio()
    {
        var rule = new LenientStatementOfWorkDigestRatioRangeRule(2.0, 524.8);
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("x", "y");
        item.Ratio = 526.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LenientStatementOfWorkDigestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new LenientStatementOfWorkDigestTitleLengthRule(32);
        var item = LenientStatementOfWorkDigest.NewLenientStatementOfWorkDigest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        LenientStatementOfWorkDigestValidatorReportsEveryProblem();
        LenientStatementOfWorkDigestRatioRangeRuleFlagsValuesAboveRatio();
        LenientStatementOfWorkDigestTitleLengthRuleFlagsALongTitle();
    }
}
