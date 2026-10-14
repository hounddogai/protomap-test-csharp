namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves seasonal warranty digests through their life cycle and keeps a journal of what it did.</summary>
public sealed record SeasonalWarrantyDigestWorkflow(ISeasonalWarrantyDigestStore Store, SeasonalWarrantyDigestValidator Validator, IClock Clock, List<string> Journal, SeasonalWarrantyDigestEngine Engine)
{
    public static SeasonalWarrantyDigestWorkflow NewSeasonalWarrantyDigestWorkflow(ISeasonalWarrantyDigestStore store, IClock clock)
    {
        return new SeasonalWarrantyDigestWorkflow(
            store,
            SeasonalWarrantyDigestRules.DefaultSeasonalWarrantyDigestValidator(),
            clock,
            new List<string>(),
            SeasonalWarrantyDigestEngine.DefaultSeasonalWarrantyDigestEngine());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal warranty digest {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(SeasonalWarrantyDigestStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Settle(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal warranty digest {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(SeasonalWarrantyDigestStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
    }

    public double Quote(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"seasonal warranty digest {code} not found");
        }
        var result = Engine.Evaluate(item);
        Note($"{code} evaluated at {result}");
        return result;
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
