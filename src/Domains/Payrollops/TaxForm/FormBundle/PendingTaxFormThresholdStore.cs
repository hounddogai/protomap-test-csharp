namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending tax form thresholds by code.</summary>
public interface IPendingTaxFormThresholdStore
{
    PendingTaxFormThreshold? Find(string code);
    void Save(PendingTaxFormThreshold item);
    bool Remove(string code);
    List<PendingTaxFormThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingTaxFormThresholdStore(Dictionary<string, PendingTaxFormThreshold> items, List<string> order) : IPendingTaxFormThresholdStore
{
    public Dictionary<string, PendingTaxFormThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingTaxFormThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingTaxFormThreshold item)
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

    public List<PendingTaxFormThreshold> List()
    {
        var outValue = new List<PendingTaxFormThreshold>();
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
public sealed class CachedPendingTaxFormThresholdStore(MemoryPendingTaxFormThresholdStore inner, PendingTaxFormThresholdCache cache, int hits, int misses) : IPendingTaxFormThresholdStore
{
    public MemoryPendingTaxFormThresholdStore Inner { get; set; } = inner;
    public PendingTaxFormThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingTaxFormThreshold? Find(string code)
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

    public void Save(PendingTaxFormThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingTaxFormThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending tax form thresholds. Empty fields match all.</summary>
public sealed record PendingTaxFormThresholdQuery(string Region, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PendingTaxFormThreshold item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingTaxFormThreshold> Apply(List<PendingTaxFormThreshold> items)
    {
        var matched = Collections.Filter(items, (PendingTaxFormThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending tax form threshold store.</summary>
public static class PendingTaxFormThresholdStore
{
    public static IPendingTaxFormThresholdStore NewMemoryPendingTaxFormThresholdStore()
    {
        return new MemoryPendingTaxFormThresholdStore(
            new Dictionary<string, PendingTaxFormThreshold>(),
            new List<string>());
    }

    public static IPendingTaxFormThresholdStore NewCachedPendingTaxFormThresholdStore(MemoryPendingTaxFormThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedPendingTaxFormThresholdStore(
            inner,
            PendingTaxFormThresholdCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingTaxFormThreshold> FindPendingTaxFormThresholds(IPendingTaxFormThresholdStore store, PendingTaxFormThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
