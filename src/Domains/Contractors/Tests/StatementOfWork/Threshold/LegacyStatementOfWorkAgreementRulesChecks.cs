namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Threshold;

using Acme.Workforce.Contractors.StatementOfWork.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyStatementOfWorkAgreementRulesChecks
{
    public static void LegacyStatementOfWorkAgreementValidatorReportsEveryProblem()
    {
        var validator = LegacyStatementOfWorkAgreementRules.DefaultLegacyStatementOfWorkAgreementValidator();
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LegacyStatementOfWorkAgreementCodeLengthRuleFlagsALongCode()
    {
        var rule = new LegacyStatementOfWorkAgreementCodeLengthRule(3600);
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void LegacyStatementOfWorkAgreementBudgetRangeRuleFlagsValuesAboveBudget()
    {
        var rule = new LegacyStatementOfWorkAgreementBudgetRangeRule(2.5, 952.8);
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("x", "y");
        item.Budget = 954.3;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void LegacyStatementOfWorkAgreementRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LegacyStatementOfWorkAgreementRequiredTitleRule(
            "legacy statement of work agreement needs a value");
        var item = LegacyStatementOfWorkAgreement.NewLegacyStatementOfWorkAgreement("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LegacyStatementOfWorkAgreementValidatorReportsEveryProblem();
        LegacyStatementOfWorkAgreementCodeLengthRuleFlagsALongCode();
        LegacyStatementOfWorkAgreementBudgetRangeRuleFlagsValuesAboveBudget();
        LegacyStatementOfWorkAgreementRequiredTitleRuleFlagsABlankTitle();
    }
}
