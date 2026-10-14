namespace Acme.Workforce.Orgdesign.Reorg.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending reorg entries by code.</summary>
public interface IPendingReorgEntryStore
{
    PendingReorgEntry? Find(string code);
    void Save(PendingReorgEntry item);
    bool Remove(string code);
    List<PendingReorgEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingReorgEntryStore(Dictionary<string, PendingReorgEntry> items, List<string> order) : IPendingReorgEntryStore
{
    public Dictionary<string, PendingReorgEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingReorgEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingReorgEntry item)
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

    public List<PendingReorgEntry> List()
    {
        var outValue = new List<PendingReorgEntry>();
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
public sealed class CachedPendingReorgEntryStore(MemoryPendingReorgEntryStore inner, PendingReorgEntryCache cache, int hits, int misses) : IPendingReorgEntryStore
{
    public MemoryPendingReorgEntryStore Inner { get; set; } = inner;
    public PendingReorgEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingReorgEntry? Find(string code)
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

    public void Save(PendingReorgEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingReorgEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending reorg entries. Empty fields match all.</summary>
public sealed record PendingReorgEntryQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PendingReorgEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingReorgEntry> Apply(List<PendingReorgEntry> items)
    {
        var matched = Collections.Filter(items, (PendingReorgEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending reorg entry store.</summary>
public static class PendingReorgEntryStore
{
    public static IPendingReorgEntryStore NewMemoryPendingReorgEntryStore()
    {
        return new MemoryPendingReorgEntryStore(new Dictionary<string, PendingReorgEntry>(), new List<string>());
    }

    public static IPendingReorgEntryStore NewCachedPendingReorgEntryStore(MemoryPendingReorgEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPendingReorgEntryStore(inner, PendingReorgEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PendingReorgEntry> FindPendingReorgEntries(IPendingReorgEntryStore store, PendingReorgEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
