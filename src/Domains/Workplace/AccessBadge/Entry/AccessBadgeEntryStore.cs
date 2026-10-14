namespace Acme.Workforce.Workplace.AccessBadge.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps access badge entries by code.</summary>
public interface IAccessBadgeEntryStore
{
    AccessBadgeEntry? Find(string code);
    void Save(AccessBadgeEntry item);
    bool Remove(string code);
    List<AccessBadgeEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAccessBadgeEntryStore(Dictionary<string, AccessBadgeEntry> items, List<string> order) : IAccessBadgeEntryStore
{
    public Dictionary<string, AccessBadgeEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AccessBadgeEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AccessBadgeEntry item)
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

    public List<AccessBadgeEntry> List()
    {
        var outValue = new List<AccessBadgeEntry>();
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
public sealed class CachedAccessBadgeEntryStore(MemoryAccessBadgeEntryStore inner, AccessBadgeEntryCache cache, int hits, int misses) : IAccessBadgeEntryStore
{
    public MemoryAccessBadgeEntryStore Inner { get; set; } = inner;
    public AccessBadgeEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AccessBadgeEntry? Find(string code)
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

    public void Save(AccessBadgeEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AccessBadgeEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects access badge entries. Empty fields match all.</summary>
public sealed record AccessBadgeEntryQuery(string Team, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(AccessBadgeEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AccessBadgeEntry> Apply(List<AccessBadgeEntry> items)
    {
        var matched = Collections.Filter(items, (AccessBadgeEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of access badge entry store.</summary>
public static class AccessBadgeEntryStore
{
    public static IAccessBadgeEntryStore NewMemoryAccessBadgeEntryStore()
    {
        return new MemoryAccessBadgeEntryStore(new Dictionary<string, AccessBadgeEntry>(), new List<string>());
    }

    public static IAccessBadgeEntryStore NewCachedAccessBadgeEntryStore(MemoryAccessBadgeEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAccessBadgeEntryStore(inner, AccessBadgeEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AccessBadgeEntry> FindAccessBadgeEntries(IAccessBadgeEntryStore store, AccessBadgeEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
