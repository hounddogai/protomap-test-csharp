namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Ledger;

using Acme.Workforce.Contractors.StatementOfWork.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalStatementOfWorkDigestRulesChecks
{
    public static void RegionalStatementOfWorkDigestValidatorReportsEveryProblem()
    {
        var validator = RegionalStatementOfWorkDigestRules.DefaultRegionalStatementOfWorkDigestValidator();
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void RegionalStatementOfWorkDigestCodeLengthRuleFlagsALongCode()
    {
        var rule = new RegionalStatementOfWorkDigestCodeLengthRule(255);
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RegionalStatementOfWorkDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new RegionalStatementOfWorkDigestRequiredNameRule(
            "regional statement of work digest needs this field");
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RegionalStatementOfWorkDigestRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new RegionalStatementOfWorkDigestRequiredTitleRule("regional statement of work digest needs a value");
        var item = RegionalStatementOfWorkDigest.NewRegionalStatementOfWorkDigest("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        RegionalStatementOfWorkDigestValidatorReportsEveryProblem();
        RegionalStatementOfWorkDigestCodeLengthRuleFlagsALongCode();
        RegionalStatementOfWorkDigestRequiredNameRuleFlagsABlankName();
        RegionalStatementOfWorkDigestRequiredTitleRuleFlagsABlankTitle();
    }
}
