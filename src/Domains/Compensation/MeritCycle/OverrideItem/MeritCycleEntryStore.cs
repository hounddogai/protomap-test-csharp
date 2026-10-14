namespace Acme.Workforce.Compensation.MeritCycle.OverrideItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle entries by code.</summary>
public interface IMeritCycleEntryStore
{
    MeritCycleEntry? Find(string code);
    void Save(MeritCycleEntry item);
    bool Remove(string code);
    List<MeritCycleEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleEntryStore(Dictionary<string, MeritCycleEntry> items, List<string> order) : IMeritCycleEntryStore
{
    public Dictionary<string, MeritCycleEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleEntry item)
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

    public List<MeritCycleEntry> List()
    {
        var outValue = new List<MeritCycleEntry>();
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
public sealed class CachedMeritCycleEntryStore(MemoryMeritCycleEntryStore inner, MeritCycleEntryCache cache, int hits, int misses) : IMeritCycleEntryStore
{
    public MemoryMeritCycleEntryStore Inner { get; set; } = inner;
    public MeritCycleEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleEntry? Find(string code)
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

    public void Save(MeritCycleEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle entries. Empty fields match all.</summary>
public sealed record MeritCycleEntryQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleEntry> Apply(List<MeritCycleEntry> items)
    {
        var matched = Collections.Filter(items, (MeritCycleEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle entry store.</summary>
public static class MeritCycleEntryStore
{
    public static IMeritCycleEntryStore NewMemoryMeritCycleEntryStore()
    {
        return new MemoryMeritCycleEntryStore(new Dictionary<string, MeritCycleEntry>(), new List<string>());
    }

    public static IMeritCycleEntryStore NewCachedMeritCycleEntryStore(MemoryMeritCycleEntryStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleEntryStore(inner, MeritCycleEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MeritCycleEntry> FindMeritCycleEntries(IMeritCycleEntryStore store, MeritCycleEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
