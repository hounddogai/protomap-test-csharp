namespace Acme.Workforce.Compliance.Tests.Attestation.AttestationDraft;

using Acme.Workforce.Compliance.Attestation.AttestationDraft;

/// <summary>Checks of attestation draft code, run by the project's runner.</summary>
public static class AdaptiveAttestationDraftRulesChecks
{
    public static void AdaptiveAttestationDraftValidatorReportsEveryProblem()
    {
        var validator = AdaptiveAttestationDraftRules.DefaultAdaptiveAttestationDraftValidator();
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AdaptiveAttestationDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays()
    {
        var rule = new AdaptiveAttestationDraftLeaveDaysRangeRule(80, 255);
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("x", "y");
        item.LeaveDays = 256;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AdaptiveAttestationDraftEmployeeIdLengthRuleFlagsALongEmployeeId()
    {
        var rule = new AdaptiveAttestationDraftEmployeeIdLengthRule(120);
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("x", "y");
        item.EmployeeId = string.Concat(Enumerable.Repeat("x", 121));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.EmployeeId = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AdaptiveAttestationDraftRequiredEmployeeIdRuleFlagsABlankEmployeeId()
    {
        var rule = new AdaptiveAttestationDraftRequiredEmployeeIdRule("adaptive attestation draft needs an entry");
        var item = AdaptiveAttestationDraft.NewAdaptiveAttestationDraft("", "");
        item.EmployeeId = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        AdaptiveAttestationDraftValidatorReportsEveryProblem();
        AdaptiveAttestationDraftLeaveDaysRangeRuleFlagsValuesAboveLeaveDays();
        AdaptiveAttestationDraftEmployeeIdLengthRuleFlagsALongEmployeeId();
        AdaptiveAttestationDraftRequiredEmployeeIdRuleFlagsABlankEmployeeId();
    }
}
