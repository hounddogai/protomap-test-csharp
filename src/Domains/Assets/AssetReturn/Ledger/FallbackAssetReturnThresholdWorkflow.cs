namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback asset return thresholds through their life cycle and keeps a journal of what it
/// did.</summary>
public sealed record FallbackAssetReturnThresholdService(IFallbackAssetReturnThresholdStore Store, FallbackAssetReturnThresholdValidator Validator, IClock Clock, List<string> Journal)
{
    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Submit(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback asset return threshold {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackAssetReturnThresholdStatus.InReview);
        Store.Save(item);
        Note($"{code} is in review");
    }

    public void Complete(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback asset return threshold {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackAssetReturnThresholdStatus.Archived);
        Store.Save(item);
        Note($"{code} is archived: {reason}");
    }

    public int Audit(int cutoff)
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

/// <summary>Functions of fallback asset return threshold workflow.</summary>
public static class FallbackAssetReturnThresholdWorkflow
{
    public static FallbackAssetReturnThresholdService NewFallbackAssetReturnThresholdService(IFallbackAssetReturnThresholdStore store, IClock clock)
    {
        return new FallbackAssetReturnThresholdService(
            store,
            FallbackAssetReturnThresholdRules.DefaultFallbackAssetReturnThresholdValidator(),
            clock,
            new List<string>());
    }
}
