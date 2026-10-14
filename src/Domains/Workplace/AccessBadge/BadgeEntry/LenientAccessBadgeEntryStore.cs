namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient access badge entries by code.</summary>
public interface ILenientAccessBadgeEntryStore
{
    LenientAccessBadgeEntry? Find(string code);
    void Save(LenientAccessBadgeEntry item);
    bool Remove(string code);
    List<LenientAccessBadgeEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientAccessBadgeEntryStore(Dictionary<string, LenientAccessBadgeEntry> items, List<string> order) : ILenientAccessBadgeEntryStore
{
    public Dictionary<string, LenientAccessBadgeEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientAccessBadgeEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientAccessBadgeEntry item)
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

    public List<LenientAccessBadgeEntry> List()
    {
        var outValue = new List<LenientAccessBadgeEntry>();
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
public sealed class CachedLenientAccessBadgeEntryStore(MemoryLenientAccessBadgeEntryStore inner, LenientAccessBadgeEntryCache cache, int hits, int misses) : ILenientAccessBadgeEntryStore
{
    public MemoryLenientAccessBadgeEntryStore Inner { get; set; } = inner;
    public LenientAccessBadgeEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientAccessBadgeEntry? Find(string code)
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

    public void Save(LenientAccessBadgeEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientAccessBadgeEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient access badge entries. Empty fields match all.</summary>
public sealed record LenientAccessBadgeEntryQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(LenientAccessBadgeEntry item)
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

    public List<LenientAccessBadgeEntry> Apply(List<LenientAccessBadgeEntry> items)
    {
        var matched = Collections.Filter(items, (LenientAccessBadgeEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient access badge entry store.</summary>
public static class LenientAccessBadgeEntryStore
{
    public static ILenientAccessBadgeEntryStore NewMemoryLenientAccessBadgeEntryStore()
    {
        return new MemoryLenientAccessBadgeEntryStore(
            new Dictionary<string, LenientAccessBadgeEntry>(),
            new List<string>());
    }

    public static ILenientAccessBadgeEntryStore NewCachedLenientAccessBadgeEntryStore(MemoryLenientAccessBadgeEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientAccessBadgeEntryStore(
            inner,
            LenientAccessBadgeEntryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<LenientAccessBadgeEntry> FindLenientAccessBadgeEntries(ILenientAccessBadgeEntryStore store, LenientAccessBadgeEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
