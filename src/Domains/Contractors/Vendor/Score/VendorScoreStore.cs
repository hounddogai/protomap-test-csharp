namespace Acme.Workforce.Contractors.Vendor.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps vendor scores by code.</summary>
public interface IVendorScoreStore
{
    VendorScore? Find(string code);
    void Save(VendorScore item);
    bool Remove(string code);
    List<VendorScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVendorScoreStore(Dictionary<string, VendorScore> items, List<string> order) : IVendorScoreStore
{
    public Dictionary<string, VendorScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VendorScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VendorScore item)
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

    public List<VendorScore> List()
    {
        var outValue = new List<VendorScore>();
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
public sealed class CachedVendorScoreStore(MemoryVendorScoreStore inner, VendorScoreCache cache, int hits, int misses) : IVendorScoreStore
{
    public MemoryVendorScoreStore Inner { get; set; } = inner;
    public VendorScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VendorScore? Find(string code)
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

    public void Save(VendorScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VendorScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects vendor scores. Empty fields match all.</summary>
public sealed record VendorScoreQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(VendorScore item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VendorScore> Apply(List<VendorScore> items)
    {
        var matched = Collections.Filter(items, (VendorScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of vendor score store.</summary>
public static class VendorScoreStore
{
    public static IVendorScoreStore NewMemoryVendorScoreStore()
    {
        return new MemoryVendorScoreStore(new Dictionary<string, VendorScore>(), new List<string>());
    }

    public static IVendorScoreStore NewCachedVendorScoreStore(MemoryVendorScoreStore inner, int ttl, IClock clock)
    {
        return new CachedVendorScoreStore(inner, VendorScoreCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VendorScore> FindVendorScores(IVendorScoreStore store, VendorScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
