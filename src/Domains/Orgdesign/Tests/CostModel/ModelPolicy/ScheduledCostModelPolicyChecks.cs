namespace Acme.Workforce.Orgdesign.Tests.CostModel.ModelPolicy;

using Acme.Workforce.Orgdesign.CostModel.ModelPolicy;

/// <summary>Checks of model policy code, run by the project's runner.</summary>
public static class ScheduledCostModelPolicyChecks
{
    public static void ScheduledCostModelPolicyStartsInCreated()
    {
        var item = ScheduledCostModelPolicy.NewScheduledCostModelPolicy("code-932", "title-170");
        Check.Equal(item.Status, ScheduledCostModelPolicyStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledCostModelPolicyMovesToValidated()
    {
        var item = ScheduledCostModelPolicy.NewScheduledCostModelPolicy("code-932", "title-170");
        try
        {
            item.MoveTo(ScheduledCostModelPolicyStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledCostModelPolicyStatus.Validated, "status");
    }

    public static void ScheduledCostModelPolicyRejectsMovesFromRetired()
    {
        var item = ScheduledCostModelPolicy.NewScheduledCostModelPolicy("code-932", "title-170");
        item.Status = ScheduledCostModelPolicyStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledCostModelPolicyStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledCostModelPolicyStartsInCreated();
        ScheduledCostModelPolicyMovesToValidated();
        ScheduledCostModelPolicyRejectsMovesFromRetired();
    }
}
