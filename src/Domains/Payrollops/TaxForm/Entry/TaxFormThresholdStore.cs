namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tax form thresholds by code.</summary>
public interface ITaxFormThresholdStore
{
    TaxFormThreshold? Find(string code);
    void Save(TaxFormThreshold item);
    bool Remove(string code);
    List<TaxFormThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTaxFormThresholdStore(Dictionary<string, TaxFormThreshold> items, List<string> order) : ITaxFormThresholdStore
{
    public Dictionary<string, TaxFormThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TaxFormThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TaxFormThreshold item)
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

    public List<TaxFormThreshold> List()
    {
        var outValue = new List<TaxFormThreshold>();
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
public sealed class CachedTaxFormThresholdStore(MemoryTaxFormThresholdStore inner, TaxFormThresholdCache cache, int hits, int misses) : ITaxFormThresholdStore
{
    public MemoryTaxFormThresholdStore Inner { get; set; } = inner;
    public TaxFormThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TaxFormThreshold? Find(string code)
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

    public void Save(TaxFormThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TaxFormThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tax form thresholds. Empty fields match all.</summary>
public sealed record TaxFormThresholdQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(TaxFormThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TaxFormThreshold> Apply(List<TaxFormThreshold> items)
    {
        var matched = Collections.Filter(items, (TaxFormThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tax form threshold store.</summary>
public static class TaxFormThresholdStore
{
    public static ITaxFormThresholdStore NewMemoryTaxFormThresholdStore()
    {
        return new MemoryTaxFormThresholdStore(new Dictionary<string, TaxFormThreshold>(), new List<string>());
    }

    public static ITaxFormThresholdStore NewCachedTaxFormThresholdStore(MemoryTaxFormThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedTaxFormThresholdStore(inner, TaxFormThresholdCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TaxFormThreshold> FindTaxFormThresholds(ITaxFormThresholdStore store, TaxFormThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
