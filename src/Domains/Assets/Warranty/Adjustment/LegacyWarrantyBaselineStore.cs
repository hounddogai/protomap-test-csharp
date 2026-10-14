namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy warranty baselines by code.</summary>
public interface ILegacyWarrantyBaselineStore
{
    LegacyWarrantyBaseline? Find(string code);
    void Save(LegacyWarrantyBaseline item);
    bool Remove(string code);
    List<LegacyWarrantyBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyWarrantyBaselineStore(Dictionary<string, LegacyWarrantyBaseline> items, List<string> order) : ILegacyWarrantyBaselineStore
{
    public Dictionary<string, LegacyWarrantyBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyWarrantyBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyWarrantyBaseline item)
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

    public List<LegacyWarrantyBaseline> List()
    {
        var outValue = new List<LegacyWarrantyBaseline>();
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
public sealed class CachedLegacyWarrantyBaselineStore(MemoryLegacyWarrantyBaselineStore inner, LegacyWarrantyBaselineCache cache, int hits, int misses) : ILegacyWarrantyBaselineStore
{
    public MemoryLegacyWarrantyBaselineStore Inner { get; set; } = inner;
    public LegacyWarrantyBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyWarrantyBaseline? Find(string code)
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

    public void Save(LegacyWarrantyBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyWarrantyBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy warranty baselines. Empty fields match all.</summary>
public sealed record LegacyWarrantyBaselineQuery(string Region, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyWarrantyBaseline item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyWarrantyBaseline> Apply(List<LegacyWarrantyBaseline> items)
    {
        var matched = Collections.Filter(items, (LegacyWarrantyBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy warranty baseline store.</summary>
public static class LegacyWarrantyBaselineStore
{
    public static ILegacyWarrantyBaselineStore NewMemoryLegacyWarrantyBaselineStore()
    {
        return new MemoryLegacyWarrantyBaselineStore(
            new Dictionary<string, LegacyWarrantyBaseline>(),
            new List<string>());
    }

    public static ILegacyWarrantyBaselineStore NewCachedLegacyWarrantyBaselineStore(MemoryLegacyWarrantyBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyWarrantyBaselineStore(
            inner,
            LegacyWarrantyBaselineCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyWarrantyBaseline> FindLegacyWarrantyBaselines(ILegacyWarrantyBaselineStore store, LegacyWarrantyBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
