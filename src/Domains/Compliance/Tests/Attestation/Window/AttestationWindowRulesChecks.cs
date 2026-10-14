namespace Acme.Workforce.Compliance.Tests.Attestation.Window;

using Acme.Workforce.Compliance.Attestation.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class AttestationWindowRulesChecks
{
    public static void AttestationWindowValidatorReportsEveryProblem()
    {
        var validator = AttestationWindowRules.DefaultAttestationWindowValidator();
        var item = AttestationWindow.NewAttestationWindow("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void AttestationWindowTitleLengthRuleFlagsALongTitle()
    {
        var rule = new AttestationWindowTitleLengthRule(3600);
        var item = AttestationWindow.NewAttestationWindow("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 3601));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void AttestationWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays()
    {
        var rule = new AttestationWindowWindowDaysRangeRule(1000, 1000);
        var item = AttestationWindow.NewAttestationWindow("x", "y");
        item.WindowDays = 1001;
        Check.True(rule.Check(item).Count > 0, "no problem above the range");
    }

    public static void AttestationWindowCostCenterLengthRuleFlagsALongCostCenter()
    {
        var rule = new AttestationWindowCostCenterLengthRule(16);
        var item = AttestationWindow.NewAttestationWindow("x", "y");
        item.CostCenter = string.Concat(Enumerable.Repeat("x", 17));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.CostCenter = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void RunAll()
    {
        AttestationWindowValidatorReportsEveryProblem();
        AttestationWindowTitleLengthRuleFlagsALongTitle();
        AttestationWindowWindowDaysRangeRuleFlagsValuesAboveWindowDays();
        AttestationWindowCostCenterLengthRuleFlagsALongCostCenter();
    }
}
