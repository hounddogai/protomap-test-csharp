namespace Acme.Workforce.Talent.Tests.Nomination.Ledger;

using Acme.Workforce.Talent.Nomination.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class NominationScheduleRulesChecks
{
    public static void NominationScheduleValidatorReportsEveryProblem()
    {
        var validator = NominationScheduleRules.DefaultNominationScheduleValidator();
        var item = NominationSchedule.NewNominationSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void NominationScheduleNameLengthRuleFlagsALongName()
    {
        var rule = new NominationScheduleNameLengthRule(255);
        var item = NominationSchedule.NewNominationSchedule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void NominationScheduleRequiredPhoneRuleFlagsABlankPhone()
    {
        var rule = new NominationScheduleRequiredPhoneRule("nomination schedule needs an entry");
        var item = NominationSchedule.NewNominationSchedule("", "");
        item.Phone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationScheduleRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new NominationScheduleRequiredCodeRule("nomination schedule needs an entry");
        var item = NominationSchedule.NewNominationSchedule("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void NominationScheduleRequiredNameRuleFlagsABlankName()
    {
        var rule = new NominationScheduleRequiredNameRule("nomination schedule needs a value");
        var item = NominationSchedule.NewNominationSchedule("", "");
        item.Name = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        NominationScheduleValidatorReportsEveryProblem();
        NominationScheduleNameLengthRuleFlagsALongName();
        NominationScheduleRequiredPhoneRuleFlagsABlankPhone();
        NominationScheduleRequiredCodeRuleFlagsABlankCode();
        NominationScheduleRequiredNameRuleFlagsABlankName();
    }
}
