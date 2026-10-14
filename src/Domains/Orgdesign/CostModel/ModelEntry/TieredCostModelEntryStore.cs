namespace Acme.Workforce.Orgdesign.CostModel.ModelEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered cost model entries by code.</summary>
public interface ITieredCostModelEntryStore
{
    TieredCostModelEntry? Find(string code);
    void Save(TieredCostModelEntry item);
    bool Remove(string code);
    List<TieredCostModelEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredCostModelEntryStore(Dictionary<string, TieredCostModelEntry> items, List<string> order) : ITieredCostModelEntryStore
{
    public Dictionary<string, TieredCostModelEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredCostModelEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredCostModelEntry item)
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

    public List<TieredCostModelEntry> List()
    {
        var outValue = new List<TieredCostModelEntry>();
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
public sealed class CachedTieredCostModelEntryStore(MemoryTieredCostModelEntryStore inner, TieredCostModelEntryCache cache, int hits, int misses) : ITieredCostModelEntryStore
{
    public MemoryTieredCostModelEntryStore Inner { get; set; } = inner;
    public TieredCostModelEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredCostModelEntry? Find(string code)
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

    public void Save(TieredCostModelEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredCostModelEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered cost model entries. Empty fields match all.</summary>
public sealed record TieredCostModelEntryQuery(string Region, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(TieredCostModelEntry item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredCostModelEntry> Apply(List<TieredCostModelEntry> items)
    {
        var matched = Collections.Filter(items, (TieredCostModelEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered cost model entry store.</summary>
public static class TieredCostModelEntryStore
{
    public static ITieredCostModelEntryStore NewMemoryTieredCostModelEntryStore()
    {
        return new MemoryTieredCostModelEntryStore(
            new Dictionary<string, TieredCostModelEntry>(),
            new List<string>());
    }

    public static ITieredCostModelEntryStore NewCachedTieredCostModelEntryStore(MemoryTieredCostModelEntryStore inner, int ttl, IClock clock)
    {
        return new CachedTieredCostModelEntryStore(inner, TieredCostModelEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredCostModelEntry> FindTieredCostModelEntries(ITieredCostModelEntryStore store, TieredCostModelEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
