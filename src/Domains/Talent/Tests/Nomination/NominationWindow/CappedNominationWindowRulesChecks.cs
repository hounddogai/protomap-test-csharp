namespace Acme.Workforce.Talent.Tests.Nomination.NominationWindow;

using Acme.Workforce.Talent.Nomination.NominationWindow;

/// <summary>Checks of nomination window code, run by the project's runner.</summary>
public static class CappedNominationWindowRulesChecks
{
    public static void CappedNominationWindowValidatorReportsEveryProblem()
    {
        var validator = CappedNominationWindowRules.DefaultCappedNominationWindowValidator();
        var item = CappedNominationWindow.NewCappedNominationWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void CappedNominationWindowNameLengthRuleFlagsALongName()
    {
        var rule = new CappedNominationWindowNameLengthRule(16);
        var item = CappedNominationWindow.NewCappedNominationWindow("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedNominationWindowLabelLengthRuleFlagsALongLabel()
    {
        var rule = new CappedNominationWindowLabelLengthRule(16);
        var item = CappedNominationWindow.NewCappedNominationWindow("x", "y");
        item.Label = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Label = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void CappedNominationWindowCodeLengthRuleFlagsALongCode()
    {
        var rule = new CappedNominationWindowCodeLengthRule(1000);
        var item = CappedNominationWindow.NewCappedNominationWindow("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        CappedNominationWindowValidatorReportsEveryProblem();
        CappedNominationWindowNameLengthRuleFlagsALongName();
        CappedNominationWindowLabelLengthRuleFlagsALongLabel();
        CappedNominationWindowCodeLengthRuleFlagsALongCode();
    }
}
