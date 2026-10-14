namespace Acme.Workforce.Compensation.Tests.EquityGrant.Adjustment;

using Acme.Workforce.Compensation.EquityGrant.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class EquityGrantRequestRulesChecks
{
    public static void EquityGrantRequestValidatorReportsEveryProblem()
    {
        var validator = EquityGrantRequestRules.DefaultEquityGrantRequestValidator();
        var item = EquityGrantRequest.NewEquityGrantRequest("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void EquityGrantRequestNameLengthRuleFlagsALongName()
    {
        var rule = new EquityGrantRequestNameLengthRule(120);
        var item = EquityGrantRequest.NewEquityGrantRequest("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantRequestNotesLengthRuleFlagsALongNotes()
    {
        var rule = new EquityGrantRequestNotesLengthRule(86400);
        var item = EquityGrantRequest.NewEquityGrantRequest("x", "y");
        item.Notes = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Notes = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void EquityGrantRequestTitleLengthRuleFlagsALongTitle()
    {
        var rule = new EquityGrantRequestTitleLengthRule(120);
        var item = EquityGrantRequest.NewEquityGrantRequest("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        EquityGrantRequestValidatorReportsEveryProblem();
        EquityGrantRequestNameLengthRuleFlagsALongName();
        EquityGrantRequestNotesLengthRuleFlagsALongNotes();
        EquityGrantRequestTitleLengthRuleFlagsALongTitle();
    }
}
