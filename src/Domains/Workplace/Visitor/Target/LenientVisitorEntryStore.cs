namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient visitor entries by code.</summary>
public interface ILenientVisitorEntryStore
{
    LenientVisitorEntry? Find(string code);
    void Save(LenientVisitorEntry item);
    bool Remove(string code);
    List<LenientVisitorEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientVisitorEntryStore(Dictionary<string, LenientVisitorEntry> items, List<string> order) : ILenientVisitorEntryStore
{
    public Dictionary<string, LenientVisitorEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientVisitorEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientVisitorEntry item)
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

    public List<LenientVisitorEntry> List()
    {
        var outValue = new List<LenientVisitorEntry>();
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
public sealed class CachedLenientVisitorEntryStore(MemoryLenientVisitorEntryStore inner, LenientVisitorEntryCache cache, int hits, int misses) : ILenientVisitorEntryStore
{
    public MemoryLenientVisitorEntryStore Inner { get; set; } = inner;
    public LenientVisitorEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientVisitorEntry? Find(string code)
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

    public void Save(LenientVisitorEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientVisitorEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient visitor entries. Empty fields match all.</summary>
public sealed record LenientVisitorEntryQuery(string Unit, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientVisitorEntry item)
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

    public List<LenientVisitorEntry> Apply(List<LenientVisitorEntry> items)
    {
        var matched = Collections.Filter(items, (LenientVisitorEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient visitor entry store.</summary>
public static class LenientVisitorEntryStore
{
    public static ILenientVisitorEntryStore NewMemoryLenientVisitorEntryStore()
    {
        return new MemoryLenientVisitorEntryStore(new Dictionary<string, LenientVisitorEntry>(), new List<string>());
    }

    public static ILenientVisitorEntryStore NewCachedLenientVisitorEntryStore(MemoryLenientVisitorEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLenientVisitorEntryStore(inner, LenientVisitorEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LenientVisitorEntry> FindLenientVisitorEntries(ILenientVisitorEntryStore store, LenientVisitorEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
