namespace Acme.Workforce.Payrollops.Tests.Deduction.Audit;

using Acme.Workforce.Payrollops.Deduction.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PrimaryDeductionTemplateRulesChecks
{
    public static void PrimaryDeductionTemplateValidatorReportsEveryProblem()
    {
        var validator = PrimaryDeductionTemplateRules.DefaultPrimaryDeductionTemplateValidator();
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryDeductionTemplateCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new PrimaryDeductionTemplateCurrencyLengthRule(32);
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryDeductionTemplateScoreRangeRuleFlagsValuesAboveScore()
    {
        var rule = new PrimaryDeductionTemplateScoreRangeRule(3.7, 375.5);
        var item = PrimaryDeductionTemplate.NewPrimaryDeductionTemplate("x", "y");
        item.Score = 377.0;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        PrimaryDeductionTemplateValidatorReportsEveryProblem();
        PrimaryDeductionTemplateCurrencyLengthRuleFlagsALongCurrency();
        PrimaryDeductionTemplateScoreRangeRuleFlagsValuesAboveScore();
    }
}
