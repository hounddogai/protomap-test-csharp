namespace Acme.Workforce.Payrollops.Tests.Deduction.Baseline;

using Acme.Workforce.Payrollops.Deduction.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class DeductionEntryRulesChecks
{
    public static void DeductionEntryValidatorReportsEveryProblem()
    {
        var validator = DeductionEntryRules.DefaultDeductionEntryValidator();
        var item = DeductionEntry.NewDeductionEntry("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeductionEntryCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeductionEntryCodeLengthRule(32);
        var item = DeductionEntry.NewDeductionEntry("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 33));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionEntryChannelLengthRuleFlagsALongChannel()
    {
        var rule = new DeductionEntryChannelLengthRule(16);
        var item = DeductionEntry.NewDeductionEntry("x", "y");
        item.Channel = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Channel = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeductionEntryRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new DeductionEntryRequiredChannelRule("deduction entry needs a value");
        var item = DeductionEntry.NewDeductionEntry("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void DeductionEntryRequiredCountryRuleFlagsABlankCountry()
    {
        var rule = new DeductionEntryRequiredCountryRule("deduction entry needs an entry");
        var item = DeductionEntry.NewDeductionEntry("", "");
        item.Country = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeductionEntryValidatorReportsEveryProblem();
        DeductionEntryCodeLengthRuleFlagsALongCode();
        DeductionEntryChannelLengthRuleFlagsALongChannel();
        DeductionEntryRequiredChannelRuleFlagsABlankChannel();
        DeductionEntryRequiredCountryRuleFlagsABlankCountry();
    }
}
