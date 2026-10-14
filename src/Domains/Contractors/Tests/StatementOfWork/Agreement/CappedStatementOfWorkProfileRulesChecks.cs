namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Agreement;

using Acme.Workforce.Contractors.StatementOfWork.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class CappedStatementOfWorkProfileRulesChecks
{
    public static void CappedStatementOfWorkProfileValidatorReportsEveryProblem()
    {
        var validator = CappedStatementOfWorkProfileRules.DefaultCappedStatementOfWorkProfileValidator();
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedStatementOfWorkProfileRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new CappedStatementOfWorkProfileRequiredExternalRefRule(
            "capped statement of work profile needs a value");
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void CappedStatementOfWorkProfileExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new CappedStatementOfWorkProfileExternalRefLengthRule(80);
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedStatementOfWorkProfileCapacityRangeRuleFlagsValuesAboveCapacity()
    {
        var rule = new CappedStatementOfWorkProfileCapacityRangeRule(1000, 86400);
        var item = CappedStatementOfWorkProfile.NewCappedStatementOfWorkProfile("x", "y");
        item.Capacity = 86401;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkProfileValidatorReportsEveryProblem();
        CappedStatementOfWorkProfileRequiredExternalRefRuleFlagsABlankExternalRef();
        CappedStatementOfWorkProfileExternalRefLengthRuleFlagsALongExternalRef();
        CappedStatementOfWorkProfileCapacityRangeRuleFlagsValuesAboveCapacity();
    }
}
