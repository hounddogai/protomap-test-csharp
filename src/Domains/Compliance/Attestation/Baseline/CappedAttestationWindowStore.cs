namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped attestation windows by code.</summary>
public interface ICappedAttestationWindowStore
{
    CappedAttestationWindow? Find(string code);
    void Save(CappedAttestationWindow item);
    bool Remove(string code);
    List<CappedAttestationWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedAttestationWindowStore(Dictionary<string, CappedAttestationWindow> items, List<string> order) : ICappedAttestationWindowStore
{
    public Dictionary<string, CappedAttestationWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedAttestationWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedAttestationWindow item)
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

    public List<CappedAttestationWindow> List()
    {
        var outValue = new List<CappedAttestationWindow>();
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
public sealed class CachedCappedAttestationWindowStore(MemoryCappedAttestationWindowStore inner, CappedAttestationWindowCache cache, int hits, int misses) : ICappedAttestationWindowStore
{
    public MemoryCappedAttestationWindowStore Inner { get; set; } = inner;
    public CappedAttestationWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedAttestationWindow? Find(string code)
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

    public void Save(CappedAttestationWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedAttestationWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped attestation windows. Empty fields match all.</summary>
public sealed record CappedAttestationWindowQuery(string Locale, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(CappedAttestationWindow item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedAttestationWindow> Apply(List<CappedAttestationWindow> items)
    {
        var matched = Collections.Filter(items, (CappedAttestationWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped attestation window store.</summary>
public static class CappedAttestationWindowStore
{
    public static ICappedAttestationWindowStore NewMemoryCappedAttestationWindowStore()
    {
        return new MemoryCappedAttestationWindowStore(
            new Dictionary<string, CappedAttestationWindow>(),
            new List<string>());
    }

    public static ICappedAttestationWindowStore NewCachedCappedAttestationWindowStore(MemoryCappedAttestationWindowStore inner, int ttl, IClock clock)
    {
        return new CachedCappedAttestationWindowStore(
            inner,
            CappedAttestationWindowCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedAttestationWindow> FindCappedAttestationWindows(ICappedAttestationWindowStore store, CappedAttestationWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
