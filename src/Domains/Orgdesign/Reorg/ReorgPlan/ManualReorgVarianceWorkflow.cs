namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves manual reorg variances through their life cycle and keeps a journal of what it did.</summary>
public sealed record ManualReorgVarianceDesk(IManualReorgVarianceStore Store, ManualReorgVarianceValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual reorg variance {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(ManualReorgVarianceStatus.Pending);
        Store.Save(item);
        Note($"{code} is pending");
    }

    public void Retire(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"manual reorg variance {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(ManualReorgVarianceStatus.Closed);
        Store.Save(item);
        Note($"{code} is closed: {reason}");
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

/// <summary>Functions of manual reorg variance workflow.</summary>
public static class ManualReorgVarianceWorkflow
{
    public static ManualReorgVarianceDesk NewManualReorgVarianceDesk(IManualReorgVarianceStore store, IClock clock)
    {
        return new ManualReorgVarianceDesk(
            store,
            ManualReorgVarianceRules.DefaultManualReorgVarianceValidator(),
            clock,
            new List<string>());
    }
}
