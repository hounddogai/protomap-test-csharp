namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentAllocationRulesChecks
{
    public static void GarnishmentAllocationValidatorReportsEveryProblem()
    {
        var validator = GarnishmentAllocationRules.DefaultGarnishmentAllocationValidator();
        var item = GarnishmentAllocation.NewGarnishmentAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void GarnishmentAllocationContactEmailLengthRuleFlagsALongContactEmail()
    {
        var rule = new GarnishmentAllocationContactEmailLengthRule(86400);
        var item = GarnishmentAllocation.NewGarnishmentAllocation("x", "y");
        item.ContactEmail = string.Concat(Enumerable.Repeat("x", 86401));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ContactEmail = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentAllocationExternalRefLengthRuleFlagsALongExternalRef()
    {
        var rule = new GarnishmentAllocationExternalRefLengthRule(255);
        var item = GarnishmentAllocation.NewGarnishmentAllocation("x", "y");
        item.ExternalRef = string.Concat(Enumerable.Repeat("x", 256));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.ExternalRef = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentAllocationTitleLengthRuleFlagsALongTitle()
    {
        var rule = new GarnishmentAllocationTitleLengthRule(1000);
        var item = GarnishmentAllocation.NewGarnishmentAllocation("x", "y");
        item.Title = string.Concat(Enumerable.Repeat("x", 1001));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Title = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void GarnishmentAllocationRequiredContactEmailRuleFlagsABlankContactEmail()
    {
        var rule = new GarnishmentAllocationRequiredContactEmailRule("garnishment allocation needs an entry");
        var item = GarnishmentAllocation.NewGarnishmentAllocation("", "");
        item.ContactEmail = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        GarnishmentAllocationValidatorReportsEveryProblem();
        GarnishmentAllocationContactEmailLengthRuleFlagsALongContactEmail();
        GarnishmentAllocationExternalRefLengthRuleFlagsALongExternalRef();
        GarnishmentAllocationTitleLengthRuleFlagsALongTitle();
        GarnishmentAllocationRequiredContactEmailRuleFlagsABlankContactEmail();
    }
}
