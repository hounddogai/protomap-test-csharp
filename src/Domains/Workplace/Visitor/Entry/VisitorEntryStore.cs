namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor entries by code.</summary>
public interface IVisitorEntryStore
{
    VisitorEntry? Find(string code);
    void Save(VisitorEntry item);
    bool Remove(string code);
    List<VisitorEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorEntryStore(Dictionary<string, VisitorEntry> items, List<string> order) : IVisitorEntryStore
{
    public Dictionary<string, VisitorEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorEntry item)
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

    public List<VisitorEntry> List()
    {
        var outValue = new List<VisitorEntry>();
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
public sealed class CachedVisitorEntryStore(MemoryVisitorEntryStore inner, VisitorEntryCache cache, int hits, int misses) : IVisitorEntryStore
{
    public MemoryVisitorEntryStore Inner { get; set; } = inner;
    public VisitorEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorEntry? Find(string code)
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

    public void Save(VisitorEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor entries. Empty fields match all.</summary>
public sealed record VisitorEntryQuery(string Channel, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorEntry item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorEntry> Apply(List<VisitorEntry> items)
    {
        var matched = Collections.Filter(items, (VisitorEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor entry store.</summary>
public static class VisitorEntryStore
{
    public static IVisitorEntryStore NewMemoryVisitorEntryStore()
    {
        return new MemoryVisitorEntryStore(new Dictionary<string, VisitorEntry>(), new List<string>());
    }

    public static IVisitorEntryStore NewCachedVisitorEntryStore(MemoryVisitorEntryStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorEntryStore(inner, VisitorEntryCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<VisitorEntry> FindVisitorEntries(IVisitorEntryStore store, VisitorEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
