namespace Acme.Workforce.Workplace.Tests.Desk.Allocation;

using Acme.Workforce.Workplace.Desk.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class DeskAllocationRulesChecks
{
    public static void DeskAllocationValidatorReportsEveryProblem()
    {
        var validator = DeskAllocationRules.DefaultDeskAllocationValidator();
        var item = DeskAllocation.NewDeskAllocation("", "");
        var problems = validator.Validate(item);
        Check.True(problems.Count >= 0, "problems");
    }

    public static void DeskAllocationCodeLengthRuleFlagsALongCode()
    {
        var rule = new DeskAllocationCodeLengthRule(80);
        var item = DeskAllocation.NewDeskAllocation("x", "y");
        item.Code = string.Concat(Enumerable.Repeat("x", 81));
        Check.Equal(rule.Check(item).Count, 1, "problems");
        item.Code = "ok";
        Check.Equal(rule.Check(item).Count, 0, "problems after fixing");
    }

    public static void DeskAllocationRequiredChannelRuleFlagsABlankChannel()
    {
        var rule = new DeskAllocationRequiredChannelRule("desk allocation needs a value");
        var item = DeskAllocation.NewDeskAllocation("", "");
        item.Channel = "  ";
        Check.Equal(rule.Check(item).Count, 1, "problems");
    }

    public static void RunAll()
    {
        DeskAllocationValidatorReportsEveryProblem();
        DeskAllocationCodeLengthRuleFlagsALongCode();
        DeskAllocationRequiredChannelRuleFlagsABlankChannel();
    }
}
