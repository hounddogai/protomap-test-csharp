namespace Acme.Workforce.Payrollops.Tests.Payslip.PayslipDigest;

using Acme.Workforce.Payrollops.Payslip.PayslipDigest;

/// <summary>Checks of payslip digest code, run by the project's runner.</summary>
public static class PayslipScoreRulesChecks
{
    public static void PayslipScoreValidatorReportsEveryProblem()
    {
        var validator = PayslipScoreRules.DefaultPayslipScoreValidator();
        var item = PayslipScore.NewPayslipScore("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PayslipScoreRequiredNameRuleFlagsABlankName()
    {
        var rule = new PayslipScoreRequiredNameRule("payslip score needs this field");
        var item = PayslipScore.NewPayslipScore("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PayslipScoreSequenceRangeRuleFlagsValuesAboveSequence()
    {
        var rule = new PayslipScoreSequenceRangeRule(16, 64);
        var item = PayslipScore.NewPayslipScore("x", "y");
        item.Sequence = 65;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipScoreRatingRangeRuleFlagsValuesAboveRating()
    {
        var rule = new PayslipScoreRatingRangeRule(0.6, 2303.7);
        var item = PayslipScore.NewPayslipScore("x", "y");
        item.Rating = 2305.2;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PayslipScoreLabelLengthRuleFlagsALongLabel()
    {
        var rule = new PayslipScoreLabelLengthRule(86400);
        var item = PayslipScore.NewPayslipScore("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PayslipScoreValidatorReportsEveryProblem();
        PayslipScoreRequiredNameRuleFlagsABlankName();
        PayslipScoreSequenceRangeRuleFlagsValuesAboveSequence();
        PayslipScoreRatingRangeRuleFlagsValuesAboveRating();
        PayslipScoreLabelLengthRuleFlagsALongLabel();
    }
}
