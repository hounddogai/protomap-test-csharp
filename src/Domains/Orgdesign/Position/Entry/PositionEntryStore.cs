namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position entries by code.</summary>
public interface IPositionEntryStore
{
    PositionEntry? Find(string code);
    void Save(PositionEntry item);
    bool Remove(string code);
    List<PositionEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionEntryStore(Dictionary<string, PositionEntry> items, List<string> order) : IPositionEntryStore
{
    public Dictionary<string, PositionEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionEntry item)
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

    public List<PositionEntry> List()
    {
        var outValue = new List<PositionEntry>();
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
public sealed class CachedPositionEntryStore(MemoryPositionEntryStore inner, PositionEntryCache cache, int hits, int misses) : IPositionEntryStore
{
    public MemoryPositionEntryStore Inner { get; set; } = inner;
    public PositionEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionEntry? Find(string code)
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

    public void Save(PositionEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position entries. Empty fields match all.</summary>
public sealed record PositionEntryQuery(string Label, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PositionEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionEntry> Apply(List<PositionEntry> items)
    {
        var matched = Collections.Filter(items, (PositionEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position entry store.</summary>
public static class PositionEntryStore
{
    public static IPositionEntryStore NewMemoryPositionEntryStore()
    {
        return new MemoryPositionEntryStore(new Dictionary<string, PositionEntry>(), new List<string>());
    }

    public static IPositionEntryStore NewCachedPositionEntryStore(MemoryPositionEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPositionEntryStore(inner, PositionEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PositionEntry> FindPositionEntries(IPositionEntryStore store, PositionEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
