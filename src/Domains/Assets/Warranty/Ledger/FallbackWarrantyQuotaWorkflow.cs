namespace Acme.Workforce.Assets.Warranty.Ledger;

using Acme.Workforce.Kit.Clock;

/// <summary>Moves fallback warranty quotas through their life cycle and keeps a journal of what it did.</summary>
public sealed record FallbackWarrantyQuotaWorkflow(IFallbackWarrantyQuotaStore Store, FallbackWarrantyQuotaValidator Validator, IClock Clock, List<string> Journal)
{
    public static FallbackWarrantyQuotaWorkflow NewFallbackWarrantyQuotaWorkflow(IFallbackWarrantyQuotaStore store, IClock clock)
    {
        return new FallbackWarrantyQuotaWorkflow(
            store,
            FallbackWarrantyQuotaRules.DefaultFallbackWarrantyQuotaValidator(),
            clock,
            new List<string>());
    }

    private void Note(string message)
    {
        Journal.Add($"{Clock.Now()} {message}");
    }

    public void Open(string code)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback warranty quota {code} not found");
        }
        Validator.RequireValid(item);
        item.MoveTo(FallbackWarrantyQuotaStatus.Validated);
        Store.Save(item);
        Note($"{code} is validated");
    }

    public void Finish(string code, string reason)
    {
        if (Store.Find(code) is not { } item)
        {
            throw new InvalidOperationException($"fallback warranty quota {code} not found");
        }
        if (item.IsTerminal())
        {
            Note($"{code} already closed");
            return;
        }
        item.MoveTo(FallbackWarrantyQuotaStatus.Retired);
        Store.Save(item);
        Note($"{code} is retired: {reason}");
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
