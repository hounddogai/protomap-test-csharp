namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal warranty profiles through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalWarrantyProfileDesk(ISeasonalWarrantyProfileStore Store, SeasonalWarrantyProfileValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Start(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal warranty profile {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalWarrantyProfileStatus.Granted);
        Store.Save(item);
        Note($"{code} is granted");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal warranty profile {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalWarrantyProfileStatus.Expired);
        Store.Save(item);
        Note($"{code} is expired: {reason}");
    }

    public int ReviewStale(int cutoff)
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

/// <summary>Functions of seasonal warranty profile workflow.</summary>
public static class SeasonalWarrantyProfileWorkflow
{
    public static SeasonalWarrantyProfileDesk NewSeasonalWarrantyProfileDesk(ISeasonalWarrantyProfileStore store, IClock clock)
    {
        return new SeasonalWarrantyProfileDesk(
            store,
            SeasonalWarrantyProfileRules.DefaultSeasonalWarrantyProfileValidator(),
            clock,
            new List<string>());
    }
}
