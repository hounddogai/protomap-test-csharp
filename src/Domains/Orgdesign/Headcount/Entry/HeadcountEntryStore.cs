namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount entries by code.</summary>
public interface IHeadcountEntryStore
{
    HeadcountEntry? Find(string code);
    void Save(HeadcountEntry item);
    bool Remove(string code);
    List<HeadcountEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountEntryStore(Dictionary<string, HeadcountEntry> items, List<string> order) : IHeadcountEntryStore
{
    public Dictionary<string, HeadcountEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountEntry item)
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

    public List<HeadcountEntry> List()
    {
        var outValue = new List<HeadcountEntry>();
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
public sealed class CachedHeadcountEntryStore(MemoryHeadcountEntryStore inner, HeadcountEntryCache cache, int hits, int misses) : IHeadcountEntryStore
{
    public MemoryHeadcountEntryStore Inner { get; set; } = inner;
    public HeadcountEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountEntry? Find(string code)
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

    public void Save(HeadcountEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount entries. Empty fields match all.</summary>
public sealed record HeadcountEntryQuery(string Channel, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountEntry item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<HeadcountEntry> Apply(List<HeadcountEntry> items)
    {
        var matched = Collections.Filter(items, (HeadcountEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount entry store.</summary>
public static class HeadcountEntryStore
{
    public static IHeadcountEntryStore NewMemoryHeadcountEntryStore()
    {
        return new MemoryHeadcountEntryStore(new Dictionary<string, HeadcountEntry>(), new List<string>());
    }

    public static IHeadcountEntryStore NewCachedHeadcountEntryStore(MemoryHeadcountEntryStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountEntryStore(inner, HeadcountEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HeadcountEntry> FindHeadcountEntries(IHeadcountEntryStore store, HeadcountEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
