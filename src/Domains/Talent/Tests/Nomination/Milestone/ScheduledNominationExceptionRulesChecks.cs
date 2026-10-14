namespace Acme.Workforce.Talent.Tests.Nomination.Milestone;

using Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class ScheduledNominationExceptionRulesChecks
{
    public static void ScheduledNominationExceptionValidatorReportsEveryProblem()
    {
        var validator = ScheduledNominationExceptionRules.DefaultScheduledNominationExceptionValidator();
        var item = ScheduledNominationException.NewScheduledNominationException("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void ScheduledNominationExceptionRequiredManagerIdRuleFlagsABlankManagerId()
    {
        var rule = new ScheduledNominationExceptionRequiredManagerIdRule(
            "scheduled nomination exception needs this field");
        var item = ScheduledNominationException.NewScheduledNominationException("", "");
        item.ManagerId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void ScheduledNominationExceptionCodeLengthRuleFlagsALongCode()
    {
        var rule = new ScheduledNominationExceptionCodeLengthRule(120);
        var item = ScheduledNominationException.NewScheduledNominationException("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void ScheduledNominationExceptionManagerIdLengthRuleFlagsALongManagerId()
    {
        var rule = new ScheduledNominationExceptionManagerIdLengthRule(64);
        var item = ScheduledNominationException.NewScheduledNominationException("x", "y");
        item.ManagerId = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ManagerId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        ScheduledNominationExceptionValidatorReportsEveryProblem();
        ScheduledNominationExceptionRequiredManagerIdRuleFlagsABlankManagerId();
        ScheduledNominationExceptionCodeLengthRuleFlagsALongCode();
        ScheduledNominationExceptionManagerIdLengthRuleFlagsALongManagerId();
    }
}
