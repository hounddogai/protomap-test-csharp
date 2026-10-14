namespace Acme.Workforce.Orgdesign.Reorg.Snapshot;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves regional reorg policies through their life cycle and keeps a journal of what it did.</summary>
public sealed record RegionalReorgPolicyService(IRegionalReorgPolicyStore Store, RegionalReorgPolicyValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Advance(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional reorg policy {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(RegionalReorgPolicyStatus.Queued);
        Store.Save(item);
        Note($"{code} is queued");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"regional reorg policy {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(RegionalReorgPolicyStatus.Failed);
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

/// <summary>Functions of regional reorg policy workflow.</summary>
public static class RegionalReorgPolicyWorkflow
{
    public static RegionalReorgPolicyService NewRegionalReorgPolicyService(IRegionalReorgPolicyStore store, IClock clock)
    {
        return new RegionalReorgPolicyService(
            store,
            RegionalReorgPolicyRules.DefaultRegionalReorgPolicyValidator(),
            clock,
            new List<string>());
    }
}
