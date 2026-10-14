namespace Acme.Workforce.Contractors.Engagement.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped engagement entries by code.</summary>
public interface ICappedEngagementEntryStore
{
    CappedEngagementEntry? Find(string code);
    void Save(CappedEngagementEntry item);
    bool Remove(string code);
    List<CappedEngagementEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedEngagementEntryStore(Dictionary<string, CappedEngagementEntry> items, List<string> order) : ICappedEngagementEntryStore
{
    public Dictionary<string, CappedEngagementEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedEngagementEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedEngagementEntry item)
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

    public List<CappedEngagementEntry> List()
    {
        var outValue = new List<CappedEngagementEntry>();
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
public sealed class CachedCappedEngagementEntryStore(MemoryCappedEngagementEntryStore inner, CappedEngagementEntryCache cache, int hits, int misses) : ICappedEngagementEntryStore
{
    public MemoryCappedEngagementEntryStore Inner { get; set; } = inner;
    public CappedEngagementEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedEngagementEntry? Find(string code)
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

    public void Save(CappedEngagementEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedEngagementEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped engagement entries. Empty fields match all.</summary>
public sealed record CappedEngagementEntryQuery(string Team, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(CappedEngagementEntry item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<CappedEngagementEntry> Apply(List<CappedEngagementEntry> items)
    {
        var matched = Collections.Filter(items, (CappedEngagementEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped engagement entry store.</summary>
public static class CappedEngagementEntryStore
{
    public static ICappedEngagementEntryStore NewMemoryCappedEngagementEntryStore()
    {
        return new MemoryCappedEngagementEntryStore(
            new Dictionary<string, CappedEngagementEntry>(),
            new List<string>());
    }

    public static ICappedEngagementEntryStore NewCachedCappedEngagementEntryStore(MemoryCappedEngagementEntryStore inner, int ttl, IClock clock)
    {
        return new CachedCappedEngagementEntryStore(
            inner,
            CappedEngagementEntryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedEngagementEntry> FindCappedEngagementEntries(ICappedEngagementEntryStore store, CappedEngagementEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
