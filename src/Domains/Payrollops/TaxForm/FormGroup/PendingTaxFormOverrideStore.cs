namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending tax form overrides by code.</summary>
public interface IPendingTaxFormOverrideStore
{
    PendingTaxFormOverride? Find(string code);
    void Save(PendingTaxFormOverride item);
    bool Remove(string code);
    List<PendingTaxFormOverride> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTaxFormOverrideStore(Dictionary<string, PendingTaxFormOverride> items, List<string> order) : IPendingTaxFormOverrideStore
{
    public Dictionary<string, PendingTaxFormOverride> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTaxFormOverride? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTaxFormOverride item)
    {
        if (!Items.ContainsKey(item.Code))
        {
            Order.Add(item.Code);
        }
        Items[item.Code] = item;
    }

    public bool Remove(string code)
    {
        if (!Items.ContainsKey(code))
        {
            return false;
        }
        Items.Remove(code);
        var kept = new List<string>();
        foreach (var existing in Order)
        {
            if (existing != code)
            {
                kept.Add(existing);
            }
        }
        Order = kept;
        return true;
    }

    public List<PendingTaxFormOverride> List()
    {
        var outValue = new List<PendingTaxFormOverride>();
        foreach (var key in Order)
        {
            if (!Items.TryGetValue(key, out var found))
            {
                continue;
            }
            outValue.Add(found);
        }
        return outValue;
    }
}

/// <summary>Serves reads from a short-lived cache in front of another store.</summary>
public sealed class CachedPendingTaxFormOverrideStore(MemoryPendingTaxFormOverrideStore inner, PendingTaxFormOverrideCache cache, int hits, int misses) : IPendingTaxFormOverrideStore
{
    public MemoryPendingTaxFormOverrideStore Inner { get; set; } = inner;
    public PendingTaxFormOverrideCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTaxFormOverride? Find(string code)
    {
        if (Cache.Get(code) is not { } hit)
        {
            Misses += 1;
            if (Inner.Find(code) is not { } found)
            {
                return null;
            }
            Cache.Put(code, found);
            return found;
        }
        Hits += 1;
        return hit;
    }

    public void Save(PendingTaxFormOverride item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTaxFormOverride> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending tax form overrides. Empty fields match all.</summary>
public sealed record PendingTaxFormOverrideQuery(string Title, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTaxFormOverride item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTaxFormOverride> Apply(List<PendingTaxFormOverride> items)
    {
        var matched = Collections.Filter(items, (PendingTaxFormOverride candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending tax form override store.</summary>
public static class PendingTaxFormOverrideStore
{
    public static IPendingTaxFormOverrideStore NewMemoryPendingTaxFormOverrideStore()
    {
        return new MemoryPendingTaxFormOverrideStore(
            new Dictionary<string, PendingTaxFormOverride>(),
            new List<string>());
    }

    public static IPendingTaxFormOverrideStore NewCachedPendingTaxFormOverrideStore(MemoryPendingTaxFormOverrideStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTaxFormOverrideStore(
            inner,
            PendingTaxFormOverrideCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingTaxFormOverride> FindPendingTaxFormOverrides(IPendingTaxFormOverrideStore store, PendingTaxFormOverrideQuery query)
    {
        return query.Apply(store.List());
    }
}
