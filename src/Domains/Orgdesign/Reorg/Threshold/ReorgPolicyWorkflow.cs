namespace Acme.Workforce.Orgdesign.Reorg.Threshold;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves reorg policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record ReorgPolicyDesk(IReorgPolicyStore Store, ReorgPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ReorgPolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"reorg policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ReorgPolicyStatus.Failed);
        Store.Save(item);
        Note($"{code} is failed: {reason}");
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
            if (item.StartsAt > cutoff)
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

/// <summary>Functions of reorg policy workflow.</summary>
public static class ReorgPolicyWorkflow
{
    public static ReorgPolicyDesk NewReorgPolicyDesk(IReorgPolicyStore store, IClock clock)
    {
        return new ReorgPolicyDesk(store, ReorgPolicyRules.DefaultReorgPolicyValidator(), clock, new List<string>());
    }
}
