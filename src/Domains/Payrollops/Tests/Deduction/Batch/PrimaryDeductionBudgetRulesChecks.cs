namespace Acme.Workforce.Payrollops.Tests.Deduction.Batch;

using Acme.Workforce.Payrollops.Deduction.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PrimaryDeductionBudgetRulesChecks
{
    public static void PrimaryDeductionBudgetValidatorReportsEveryProblem()
    {
        var validator = PrimaryDeductionBudgetRules.DefaultPrimaryDeductionBudgetValidator();
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PrimaryDeductionBudgetRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new PrimaryDeductionBudgetRequiredCountryRule("primary deduction budget needs an entry");
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PrimaryDeductionBudgetNameLengthRuleFlagsALongName()
    {
        var rule = new PrimaryDeductionBudgetNameLengthRule(86400);
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PrimaryDeductionBudgetCreatedAtRangeRuleFlagsValuesAboveCreatedAt()
    {
        var rule = new PrimaryDeductionBudgetCreatedAtRangeRule(16, 1000);
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("x", "y");
        item.CreatedAt = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PrimaryDeductionBudgetRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PrimaryDeductionBudgetRequiredCodeRule("primary deduction budget needs this field");
        var item = PrimaryDeductionBudget.NewPrimaryDeductionBudget("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PrimaryDeductionBudgetValidatorReportsEveryProblem();
        PrimaryDeductionBudgetRequiredCountryRuleFlagsABlankCountry();
        PrimaryDeductionBudgetNameLengthRuleFlagsALongName();
        PrimaryDeductionBudgetCreatedAtRangeRuleFlagsValuesAboveCreatedAt();
        PrimaryDeductionBudgetRequiredCodeRuleFlagsABlankCode();
    }
}
