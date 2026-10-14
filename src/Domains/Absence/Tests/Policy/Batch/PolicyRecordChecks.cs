namespace Acme.Workforce.Absence.Tests.Policy.Batch;

using Acme.Workforce.Absence.Policy.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PolicyRecordChecks
{
    public static void PolicyRecordStartsInPlanned()
    {
        var item = PolicyRecord.NewPolicyRecord("code-846", "label-977");
        Check.Equal(item.Status, PolicyRecordStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PolicyRecordMovesToCancelled()
    {
        var item = PolicyRecord.NewPolicyRecord("code-846", "label-977");
        try
        {
            item.MoveTo(PolicyRecordStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PolicyRecordStatus.Cancelled, "status");
    }

    public static void PolicyRecordRejectsMovesFromCancelled()
    {
        var item = PolicyRecord.NewPolicyRecord("code-846", "label-977");
        item.Status = PolicyRecordStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PolicyRecordStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PolicyRecordStartsInPlanned();
        PolicyRecordMovesToCancelled();
        PolicyRecordRejectsMovesFromCancelled();
    }
}
