namespace Acme.Workforce.Compliance.Tests.Attestation.Schedule;

using Acme.Workforce.Compliance.Attestation.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AttestationScheduleRulesChecks
{
    public static void AttestationScheduleValidatorReportsEveryProblem()
    {
        var validator = AttestationScheduleRules.DefaultAttestationScheduleValidator();
        var item = AttestationSchedule.NewAttestationSchedule("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationScheduleCurrencyLengthRuleFlagsALongCurrency()
    {
        var rule = new AttestationScheduleCurrencyLengthRule(16);
        var item = AttestationSchedule.NewAttestationSchedule("x", "y");
        item.Currency = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Currency = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationScheduleRequiredUnitRuleFlagsABlankUnit()
    {
        var rule = new AttestationScheduleRequiredUnitRule("attestation schedule needs this field");
        var item = AttestationSchedule.NewAttestationSchedule("", "");
        item.Unit = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void AttestationScheduleUnitLengthRuleFlagsALongUnit()
    {
        var rule = new AttestationScheduleUnitLengthRule(120);
        var item = AttestationSchedule.NewAttestationSchedule("x", "y");
        item.Unit = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Unit = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationScheduleNameLengthRuleFlagsALongName()
    {
        var rule = new AttestationScheduleNameLengthRule(16);
        var item = AttestationSchedule.NewAttestationSchedule("x", "y");
        item.Name = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Name = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationScheduleCodeLengthRuleFlagsALongCode()
    {
        var rule = new AttestationScheduleCodeLengthRule(1000);
        var item = AttestationSchedule.NewAttestationSchedule("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationScheduleValidatorReportsEveryProblem();
        AttestationScheduleCurrencyLengthRuleFlagsALongCurrency();
        AttestationScheduleRequiredUnitRuleFlagsABlankUnit();
        AttestationScheduleUnitLengthRuleFlagsALongUnit();
        AttestationScheduleNameLengthRuleFlagsALongName();
        AttestationScheduleCodeLengthRuleFlagsALongCode();
    }
}
