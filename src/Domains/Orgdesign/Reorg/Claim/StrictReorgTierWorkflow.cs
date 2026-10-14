namespace Acme.Workforce.Orgdesign.Reorg.Claim;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves strict reorg tiers through their life cycle and keeps a journal of what it did.</summary>
public sealed record StrictReorgTierWorkflow(IStrictReorgTierStore Store, StrictReorgTierValidator Validator, IClock Clock, List<string> Journal)
{
    public static StrictReorgTierWorkflow NewStrictReorgTierWorkflow(IStrictReorgTierStore store, IClock clock)
    {
        return new StrictReorgTierWorkflow(
            store,
            StrictReorgTierRules.DefaultStrictReorgTierValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg tier {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(StrictReorgTierStatus.Warming);
        Store.Save(item);
        Note($"{code} is warming");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"strict reorg tier {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(StrictReorgTierStatus.Stopped);
        Store.Save(item);
        Note($"{code} is stopped: {reason}");
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
