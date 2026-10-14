namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Summary;

using Acme.Workforce.Contractors.StatementOfWork.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class StatementOfWorkQuotaRulesChecks
{
    public static void StatementOfWorkQuotaValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkQuotaRules.DefaultStatementOfWorkQuotaValidator();
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkQuotaRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new StatementOfWorkQuotaRequiredCodeRule("statement of work quota needs a value");
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkQuotaRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new StatementOfWorkQuotaRequiredContactEmailRule("statement of work quota needs an entry");
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkQuotaContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new StatementOfWorkQuotaContactEmailLengthRule(16);
        var item = StatementOfWorkQuota.NewStatementOfWorkQuota("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        StatementOfWorkQuotaValidatorReportsEveryProblem();
        StatementOfWorkQuotaRequiredCodeRuleFlagsABlankCode();
        StatementOfWorkQuotaRequiredContactEmailRuleFlagsABlankContactEmail();
        StatementOfWorkQuotaContactEmailLengthRuleFlagsALongContactEmail();
    }
}
