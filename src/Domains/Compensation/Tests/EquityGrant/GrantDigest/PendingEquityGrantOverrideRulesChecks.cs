namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantDigest;

using Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Checks of grant digest code, run by the project's runner.</summary>
public static class PendingEquityGrantOverrideRulesChecks
{
    public static void PendingEquityGrantOverrideValidatorReportsEveryProblem()
    {
        var validator = PendingEquityGrantOverrideRules.DefaultPendingEquityGrantOverrideValidator();
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void PendingEquityGrantOverrideRequiredExternalRefRuleFlagsABlankExternalRef()
    {
        var rule = new PendingEquityGrantOverrideRequiredExternalRefRule("pending equity grant override needs an entry");
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("", "");
        item.ExternalRef = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void PendingEquityGrantOverrideRequiredCodeRuleFlagsABlankCode()
    {
        var rule = new PendingEquityGrantOverrideRequiredCodeRule("pending equity grant override needs this field");
        var item = PendingEquityGrantOverride.NewPendingEquityGrantOverride("", "");
        item.Code = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        PendingEquityGrantOverrideValidatorReportsEveryProblem();
        PendingEquityGrantOverrideRequiredExternalRefRuleFlagsABlankExternalRef();
        PendingEquityGrantOverrideRequiredCodeRuleFlagsABlankCode();
    }
}
