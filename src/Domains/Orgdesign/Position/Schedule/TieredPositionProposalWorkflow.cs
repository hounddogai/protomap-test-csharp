namespace Acme.Workforce.Orgdesign.Position.Schedule;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves tiered position proposals through their life cycle and keeps a journal of what it did.</summary>
public sealed record TieredPositionProposalManager(ITieredPositionProposalStore Store, TieredPositionProposalValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position proposal {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(TieredPositionProposalStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Close(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"tiered position proposal {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(TieredPositionProposalStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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

/// <summary>Functions of tiered position proposal workflow.</summary>
public static class TieredPositionProposalWorkflow
{
    public static TieredPositionProposalManager NewTieredPositionProposalManager(ITieredPositionProposalStore store, IClock clock)
    {
        return new TieredPositionProposalManager(
            store,
            TieredPositionProposalRules.DefaultTieredPositionProposalValidator(),
            clock,
            new List<string>());
    }
}
