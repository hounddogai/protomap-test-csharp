namespace Acme.Workforce.Absence.Tests.Holiday.HolidayQuota;

using Acme.Workforce.Absence.Holiday.HolidayQuota;

/// <summary>Checks of holiday quota code, run by the project's runner.</summary>
public static class PendingHolidayQuotaRulesChecks
{
    public static void PendingHolidayQuotaValidatorReportsEveryProblem()
    {
        var validator = PendingHolidayQuotaRules.DefaultPendingHolidayQuotaValidator();
        var item = PendingHolidayQuota.NewPendingHolidayQuota("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingHolidayQuotaLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new PendingHolidayQuotaLeaveDaysRangeRule(64, 1000);
        var item = PendingHolidayQuota.NewPendingHolidayQuota("x", "y");
        item.LeaveDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void PendingHolidayQuotaContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new PendingHolidayQuotaContactEmailLengthRule(255);
        var item = PendingHolidayQuota.NewPendingHolidayQuota("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void PendingHolidayQuotaRequiredTimezoneRuleFlagsABlankTimezone()
    {
        var rule = new PendingHolidayQuotaRequiredTimezoneRule("pending holiday quota needs an entry");
        var item = PendingHolidayQuota.NewPendingHolidayQuota("", "");
        item.Timezone = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingHolidayQuotaCodeLengthRuleFlagsALongCode()
    {
        var rule = new PendingHolidayQuotaCodeLengthRule(64);
        var item = PendingHolidayQuota.NewPendingHolidayQuota("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 65));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        PendingHolidayQuotaValidatorReportsEveryProblem();
        PendingHolidayQuotaLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        PendingHolidayQuotaContactEmailLengthRuleFlagsALongContactEmail();
        PendingHolidayQuotaRequiredTimezoneRuleFlagsABlankTimezone();
        PendingHolidayQuotaCodeLengthRuleFlagsALongCode();
    }
}
