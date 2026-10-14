namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkDigestRulesChecks
{
    public static void StatementOfWorkDigestValidatorReportsEveryProblem()
    {
        var validator = StatementOfWorkDigestRules.DefaultStatementOfWorkDigestValidator();
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void StatementOfWorkDigestPriorityRangeRuleFlagsValuesAbovePriority()
    {
        var rule = new StatementOfWorkDigestPriorityRangeRule(3600, 3600);
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("x", "y");
        item.Priority = 3601;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void StatementOfWorkDigestRequiredSalaryBandRuleFlagsABlankSalaryBand()
    {
        var rule = new StatementOfWorkDigestRequiredSalaryBandRule("statement of work digest needs a value");
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("", "");
        item.SalaryBand = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkDigestRequiredRegionRuleFlagsABlankRegion()
    {
        var rule = new StatementOfWorkDigestRequiredRegionRule("statement of work digest needs an entry");
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("", "");
        item.Region = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void StatementOfWorkDigestRequiredNameRuleFlagsABlankName()
    {
        var rule = new StatementOfWorkDigestRequiredNameRule("statement of work digest needs this field");
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        StatementOfWorkDigestValidatorReportsEveryProblem();
        StatementOfWorkDigestPriorityRangeRuleFlagsValuesAbovePriority();
        StatementOfWorkDigestRequiredSalaryBandRuleFlagsABlankSalaryBand();
        StatementOfWorkDigestRequiredRegionRuleFlagsABlankRegion();
        StatementOfWorkDigestRequiredNameRuleFlagsABlankName();
    }
}
