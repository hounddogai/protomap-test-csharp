namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal policy ack entries by code.</summary>
public interface ISeasonalPolicyAckEntryStore
{
    SeasonalPolicyAckEntry? Find(string code);
    void Save(SeasonalPolicyAckEntry item);
    bool Remove(string code);
    List<SeasonalPolicyAckEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalPolicyAckEntryStore(Dictionary<string, SeasonalPolicyAckEntry> items, List<string> order) : ISeasonalPolicyAckEntryStore
{
    public Dictionary<string, SeasonalPolicyAckEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalPolicyAckEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalPolicyAckEntry item)
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

    public List<SeasonalPolicyAckEntry> List()
    {
        var outValue = new List<SeasonalPolicyAckEntry>();
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
public sealed class CachedSeasonalPolicyAckEntryStore(MemorySeasonalPolicyAckEntryStore inner, SeasonalPolicyAckEntryCache cache, int hits, int misses) : ISeasonalPolicyAckEntryStore
{
    public MemorySeasonalPolicyAckEntryStore Inner { get; set; } = inner;
    public SeasonalPolicyAckEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalPolicyAckEntry? Find(string code)
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

    public void Save(SeasonalPolicyAckEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalPolicyAckEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal policy ack entries. Empty fields match all.</summary>
public sealed record SeasonalPolicyAckEntryQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalPolicyAckEntry item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalPolicyAckEntry> Apply(List<SeasonalPolicyAckEntry> items)
    {
        var matched = Collections.Filter(items, (SeasonalPolicyAckEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal policy ack entry store.</summary>
public static class SeasonalPolicyAckEntryStore
{
    public static ISeasonalPolicyAckEntryStore NewMemorySeasonalPolicyAckEntryStore()
    {
        return new MemorySeasonalPolicyAckEntryStore(
            new Dictionary<string, SeasonalPolicyAckEntry>(),
            new List<string>());
    }

    public static ISeasonalPolicyAckEntryStore NewCachedSeasonalPolicyAckEntryStore(MemorySeasonalPolicyAckEntryStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalPolicyAckEntryStore(
            inner,
            SeasonalPolicyAckEntryCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalPolicyAckEntry> FindSeasonalPolicyAckEntries(ISeasonalPolicyAckEntryStore store, SeasonalPolicyAckEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
