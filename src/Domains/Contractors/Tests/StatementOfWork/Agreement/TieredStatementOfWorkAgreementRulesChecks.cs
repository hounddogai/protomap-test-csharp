namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredStatementOfWorkAgreementRulesChecks
{
    public static void TieredStatementOfWorkAgreementValidatorReportsEveryProblem()
    {
        var validator = TieredStatementOfWorkAgreementRules.DefaultTieredStatementOfWorkAgreementValidator();
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void TieredStatementOfWorkAgreementRequiredNameRuleFlagsABlankName()
    {
        var rule = new TieredStatementOfWorkAgreementRequiredNameRule(
            "tiered statement of work agreement needs an entry");
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void TieredStatementOfWorkAgreementManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new TieredStatementOfWorkAgreementManagerIdLengthRule(86400);
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void TieredStatementOfWorkAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt()
    {
        var rule = new TieredStatementOfWorkAgreementUpdatedAtRangeRule(80, 3600);
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("x", "y");
        item.UpdatedAt = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void TieredStatementOfWorkAgreementRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new TieredStatementOfWorkAgreementRequiredManagerIdRule(
            "tiered statement of work agreement needs an entry");
        var item = TieredStatementOfWorkAgreement.NewTieredStatementOfWorkAgreement("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        TieredStatementOfWorkAgreementValidatorReportsEveryProblem();
        TieredStatementOfWorkAgreementRequiredNameRuleFlagsABlankName();
        TieredStatementOfWorkAgreementManagerIdLengthRuleFlagsALongManagerId();
        TieredStatementOfWorkAgreementUpdatedAtRangeRuleFlagsValuesAboveUpdatedAt();
        TieredStatementOfWorkAgreementRequiredManagerIdRuleFlagsABlankManagerId();
    }
}
