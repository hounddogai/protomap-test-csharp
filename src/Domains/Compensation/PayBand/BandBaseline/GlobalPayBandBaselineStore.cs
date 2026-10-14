namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global pay band baselines by code.</summary>
public interface IGlobalPayBandBaselineStore
{
    GlobalPayBandBaseline? Find(string code);
    void Save(GlobalPayBandBaseline item);
    bool Remove(string code);
    List<GlobalPayBandBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalPayBandBaselineStore(Dictionary<string, GlobalPayBandBaseline> items, List<string> order) : IGlobalPayBandBaselineStore
{
    public Dictionary<string, GlobalPayBandBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalPayBandBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalPayBandBaseline item)
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

    public List<GlobalPayBandBaseline> List()
    {
        var outValue = new List<GlobalPayBandBaseline>();
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
public sealed class CachedGlobalPayBandBaselineStore(MemoryGlobalPayBandBaselineStore inner, GlobalPayBandBaselineCache cache, int hits, int misses) : IGlobalPayBandBaselineStore
{
    public MemoryGlobalPayBandBaselineStore Inner { get; set; } = inner;
    public GlobalPayBandBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalPayBandBaseline? Find(string code)
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

    public void Save(GlobalPayBandBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalPayBandBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global pay band baselines. Empty fields match all.</summary>
public sealed record GlobalPayBandBaselineQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalPayBandBaseline item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalPayBandBaseline> Apply(List<GlobalPayBandBaseline> items)
    {
        var matched = Collections.Filter(items, (GlobalPayBandBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global pay band baseline store.</summary>
public static class GlobalPayBandBaselineStore
{
    public static IGlobalPayBandBaselineStore NewMemoryGlobalPayBandBaselineStore()
    {
        return new MemoryGlobalPayBandBaselineStore(
            new Dictionary<string, GlobalPayBandBaseline>(),
            new List<string>());
    }

    public static IGlobalPayBandBaselineStore NewCachedGlobalPayBandBaselineStore(MemoryGlobalPayBandBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalPayBandBaselineStore(inner, GlobalPayBandBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<GlobalPayBandBaseline> FindGlobalPayBandBaselines(IGlobalPayBandBaselineStore store, GlobalPayBandBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
