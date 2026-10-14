namespace Acme.Workforce.Orgdesign.Span.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global span entries by code.</summary>
public interface IGlobalSpanEntryStore
{
    GlobalSpanEntry? Find(string code);
    void Save(GlobalSpanEntry item);
    bool Remove(string code);
    List<GlobalSpanEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalSpanEntryStore(Dictionary<string, GlobalSpanEntry> items, List<string> order) : IGlobalSpanEntryStore
{
    public Dictionary<string, GlobalSpanEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalSpanEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalSpanEntry item)
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

    public List<GlobalSpanEntry> List()
    {
        var outValue = new List<GlobalSpanEntry>();
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
public sealed class CachedGlobalSpanEntryStore(MemoryGlobalSpanEntryStore inner, GlobalSpanEntryCache cache, int hits, int misses) : IGlobalSpanEntryStore
{
    public MemoryGlobalSpanEntryStore Inner { get; set; } = inner;
    public GlobalSpanEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalSpanEntry? Find(string code)
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

    public void Save(GlobalSpanEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalSpanEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global span entries. Empty fields match all.</summary>
public sealed record GlobalSpanEntryQuery(string Region, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalSpanEntry item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalSpanEntry> Apply(List<GlobalSpanEntry> items)
    {
        var matched = Collections.Filter(items, (GlobalSpanEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global span entry store.</summary>
public static class GlobalSpanEntryStore
{
    public static IGlobalSpanEntryStore NewMemoryGlobalSpanEntryStore()
    {
        return new MemoryGlobalSpanEntryStore(new Dictionary<string, GlobalSpanEntry>(), new List<string>());
    }

    public static IGlobalSpanEntryStore NewCachedGlobalSpanEntryStore(MemoryGlobalSpanEntryStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalSpanEntryStore(inner, GlobalSpanEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<GlobalSpanEntry> FindGlobalSpanEntries(IGlobalSpanEntryStore store, GlobalSpanEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
