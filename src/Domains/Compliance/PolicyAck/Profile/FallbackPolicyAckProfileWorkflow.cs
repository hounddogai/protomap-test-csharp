namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback policy ack profiles through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackPolicyAckProfileDesk(IFallbackPolicyAckProfileStore Store, FallbackPolicyAckProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackPolicyAckProfileStatus.Approved);
        Store.Save(item);
        Note($"{code} is approved");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback policy ack profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackPolicyAckProfileStatus.Withdrawn);
        Store.Save(item);
        Note($"{code} is withdrawn: {reason}");
    }

    public int Scan(int cutoff)
    {
        var changed = 0;
        foreach (var item in Store.List())
        {
            if (item.IsTerminal())
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

/// <summary>Functions of fallback policy ack profile workflow.</summary>
public static class FallbackPolicyAckProfileWorkflow
{
    public static FallbackPolicyAckProfileDesk NewFallbackPolicyAckProfileDesk(IFallbackPolicyAckProfileStore store, IClock clock)
    {
        return new FallbackPolicyAckProfileDesk(
            store,
            FallbackPolicyAckProfileRules.DefaultFallbackPolicyAckProfileValidator(),
            clock,
            new List<string>());
    }
}
