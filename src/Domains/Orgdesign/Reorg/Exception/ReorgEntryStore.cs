namespace Acme.Workforce.Orgdesign.Reorg.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg entries by code.</summary>
public interface IReorgEntryStore
{
    ReorgEntry? Find(string code);
    void Save(ReorgEntry item);
    bool Remove(string code);
    List<ReorgEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgEntryStore(Dictionary<string, ReorgEntry> items, List<string> order) : IReorgEntryStore
{
    public Dictionary<string, ReorgEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgEntry item)
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

    public List<ReorgEntry> List()
    {
        var outValue = new List<ReorgEntry>();
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
public sealed class CachedReorgEntryStore(MemoryReorgEntryStore inner, ReorgEntryCache cache, int hits, int misses) : IReorgEntryStore
{
    public MemoryReorgEntryStore Inner { get; set; } = inner;
    public ReorgEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgEntry? Find(string code)
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

    public void Save(ReorgEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg entries. Empty fields match all.</summary>
public sealed record ReorgEntryQuery(string Unit, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgEntry item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgEntry> Apply(List<ReorgEntry> items)
    {
        var matched = Collections.Filter(items, (ReorgEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg entry store.</summary>
public static class ReorgEntryStore
{
    public static IReorgEntryStore NewMemoryReorgEntryStore()
    {
        return new MemoryReorgEntryStore(new Dictionary<string, ReorgEntry>(), new List<string>());
    }

    public static IReorgEntryStore NewCachedReorgEntryStore(MemoryReorgEntryStore inner, int ttl, IClock clock)
    {
        return new CachedReorgEntryStore(inner, ReorgEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<ReorgEntry> FindReorgEntries(IReorgEntryStore store, ReorgEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
