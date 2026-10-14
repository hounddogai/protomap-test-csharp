namespace Acme.Workforce.Contractors.Rate.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rate entries by code.</summary>
public interface IRateEntryStore
{
    RateEntry? Find(string code);
    void Save(RateEntry item);
    bool Remove(string code);
    List<RateEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRateEntryStore(Dictionary<string, RateEntry> items, List<string> order) : IRateEntryStore
{
    public Dictionary<string, RateEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RateEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RateEntry item)
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

    public List<RateEntry> List()
    {
        var outValue = new List<RateEntry>();
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
public sealed class CachedRateEntryStore(MemoryRateEntryStore inner, RateEntryCache cache, int hits, int misses) : IRateEntryStore
{
    public MemoryRateEntryStore Inner { get; set; } = inner;
    public RateEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RateEntry? Find(string code)
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

    public void Save(RateEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RateEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rate entries. Empty fields match all.</summary>
public sealed record RateEntryQuery(string Team, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(RateEntry item)
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

    public List<RateEntry> Apply(List<RateEntry> items)
    {
        var matched = Collections.Filter(items, (RateEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rate entry store.</summary>
public static class RateEntryStore
{
    public static IRateEntryStore NewMemoryRateEntryStore()
    {
        return new MemoryRateEntryStore(new Dictionary<string, RateEntry>(), new List<string>());
    }

    public static IRateEntryStore NewCachedRateEntryStore(MemoryRateEntryStore inner, int ttl, IClock clock)
    {
        return new CachedRateEntryStore(inner, RateEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<RateEntry> FindRateEntries(IRateEntryStore store, RateEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
