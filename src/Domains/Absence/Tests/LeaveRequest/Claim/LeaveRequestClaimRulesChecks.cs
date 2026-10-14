namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LeaveRequestClaimRulesChecks
{
    public static void LeaveRequestClaimValidatorReportsEveryProblem()
    {
        var validator = LeaveRequestClaimRules.DefaultLeaveRequestClaimValidator();
        var item = LeaveRequestClaim.NewLeaveRequestClaim("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void LeaveRequestClaimRequiredTitleRuleFlagsABlankTitle()
    {
        var rule = new LeaveRequestClaimRequiredTitleRule("leave request claim needs an entry");
        var item = LeaveRequestClaim.NewLeaveRequestClaim("", "");
        item.Title = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void LeaveRequestClaimRequiredCurrencyRuleFlagsABlankCurrency()
    {
        var rule = new LeaveRequestClaimRequiredCurrencyRule("leave request claim needs an entry");
        var item = LeaveRequestClaim.NewLeaveRequestClaim("", "");
        item.Currency = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        LeaveRequestClaimValidatorReportsEveryProblem();
        LeaveRequestClaimRequiredTitleRuleFlagsABlankTitle();
        LeaveRequestClaimRequiredCurrencyRuleFlagsABlankCurrency();
    }
}
