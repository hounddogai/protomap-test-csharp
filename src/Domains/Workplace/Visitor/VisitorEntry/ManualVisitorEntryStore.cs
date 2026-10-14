namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual visitor entries by code.</summary>
public interface IManualVisitorEntryStore
{
    ManualVisitorEntry? Find(string code);
    void Save(ManualVisitorEntry item);
    bool Remove(string code);
    List<ManualVisitorEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualVisitorEntryStore(Dictionary<string, ManualVisitorEntry> items, List<string> order) : IManualVisitorEntryStore
{
    public Dictionary<string, ManualVisitorEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualVisitorEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualVisitorEntry item)
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

    public List<ManualVisitorEntry> List()
    {
        var outValue = new List<ManualVisitorEntry>();
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
public sealed class CachedManualVisitorEntryStore(MemoryManualVisitorEntryStore inner, ManualVisitorEntryCache cache, int hits, int misses) : IManualVisitorEntryStore
{
    public MemoryManualVisitorEntryStore Inner { get; set; } = inner;
    public ManualVisitorEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualVisitorEntry? Find(string code)
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

    public void Save(ManualVisitorEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualVisitorEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual visitor entries. Empty fields match all.</summary>
public sealed record ManualVisitorEntryQuery(string Segment, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(ManualVisitorEntry item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualVisitorEntry> Apply(List<ManualVisitorEntry> items)
    {
        var matched = Collections.Filter(items, (ManualVisitorEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual visitor entry store.</summary>
public static class ManualVisitorEntryStore
{
    public static IManualVisitorEntryStore NewMemoryManualVisitorEntryStore()
    {
        return new MemoryManualVisitorEntryStore(new Dictionary<string, ManualVisitorEntry>(), new List<string>());
    }

    public static IManualVisitorEntryStore NewCachedManualVisitorEntryStore(MemoryManualVisitorEntryStore inner, int ttl, IClock clock)
    {
        return new CachedManualVisitorEntryStore(inner, ManualVisitorEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ManualVisitorEntry> FindManualVisitorEntries(IManualVisitorEntryStore store, ManualVisitorEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
