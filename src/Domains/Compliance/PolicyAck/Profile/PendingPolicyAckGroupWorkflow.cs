namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Flags;

/// <summary>Moves pending policy ack groups through their life cycle and keeps a journal of what it did.</summary>
public sealed record PendingPolicyAckGroupDesk(IPendingPolicyAckGroupStore Store, PendingPolicyAckGroupValidator Validator, IClock Clock, List<string> Journal, PendingPolicyAckGroupEngine Engine)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending policy ack group {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(PendingPolicyAckGroupStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending policy ack group {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(PendingPolicyAckGroupStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
    }

    public double Score(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"pending policy ack group {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
    }

    public int Sweep(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
            {
                continue;
            }
            if (item.UpdatedAt > cutoff)
            {
                continue;
            }
            var problems = Validator.Validate(item);
            if (problems.Count == 0)
            {
                continue;
            }
            Note($"{item.Code} needs attention: {string.Join(", ", problems)}");
            changed += 1;
        }
        return changed;
    }
}

/// <summary>Functions of pending policy ack group workflow.</summary>
public static class PendingPolicyAckGroupWorkflow
{
    public static PendingPolicyAckGroupDesk NewPendingPolicyAckGroupDesk(IPendingPolicyAckGroupStore store, IClock clock, Registry flags)
    {
        return new PendingPolicyAckGroupDesk(
            store,
            PendingPolicyAckGroupRules.DefaultPendingPolicyAckGroupValidator(),
            clock,
            new List<string>(),
            PendingPolicyAckGroupEngine.DefaultPendingPolicyAckGroupEngine(flags));
    }
}
