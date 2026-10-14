namespace Acme.Workforce.Compensation.EquityGrant.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant entries by code.</summary>
public interface IEquityGrantEntryStore
{
    EquityGrantEntry? Find(string code);
    void Save(EquityGrantEntry item);
    bool Remove(string code);
    List<EquityGrantEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantEntryStore(Dictionary<string, EquityGrantEntry> items, List<string> order) : IEquityGrantEntryStore
{
    public Dictionary<string, EquityGrantEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantEntry item)
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

    public List<EquityGrantEntry> List()
    {
        var outValue = new List<EquityGrantEntry>();
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
public sealed class CachedEquityGrantEntryStore(MemoryEquityGrantEntryStore inner, EquityGrantEntryCache cache, int hits, int misses) : IEquityGrantEntryStore
{
    public MemoryEquityGrantEntryStore Inner { get; set; } = inner;
    public EquityGrantEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantEntry? Find(string code)
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

    public void Save(EquityGrantEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant entries. Empty fields match all.</summary>
public sealed record EquityGrantEntryQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantEntry item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantEntry> Apply(List<EquityGrantEntry> items)
    {
        var matched = Collections.Filter(items, (EquityGrantEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant entry store.</summary>
public static class EquityGrantEntryStore
{
    public static IEquityGrantEntryStore NewMemoryEquityGrantEntryStore()
    {
        return new MemoryEquityGrantEntryStore(new Dictionary<string, EquityGrantEntry>(), new List<string>());
    }

    public static IEquityGrantEntryStore NewCachedEquityGrantEntryStore(MemoryEquityGrantEntryStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantEntryStore(inner, EquityGrantEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<EquityGrantEntry> FindEquityGrantEntries(IEquityGrantEntryStore store, EquityGrantEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
