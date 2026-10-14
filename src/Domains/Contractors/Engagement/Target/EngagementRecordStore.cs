namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement records by code.</summary>
public interface IEngagementRecordStore
{
    EngagementRecord? Find(string code);
    void Save(EngagementRecord item);
    bool Remove(string code);
    List<EngagementRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementRecordStore(Dictionary<string, EngagementRecord> items, List<string> order) : IEngagementRecordStore
{
    public Dictionary<string, EngagementRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementRecord item)
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

    public List<EngagementRecord> List()
    {
        var outValue = new List<EngagementRecord>();
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
public sealed class CachedEngagementRecordStore(MemoryEngagementRecordStore inner, EngagementRecordCache cache, int hits, int misses) : IEngagementRecordStore
{
    public MemoryEngagementRecordStore Inner { get; set; } = inner;
    public EngagementRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementRecord? Find(string code)
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

    public void Save(EngagementRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement records. Empty fields match all.</summary>
public sealed record EngagementRecordQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementRecord item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EngagementRecord> Apply(List<EngagementRecord> items)
    {
        var matched = Collections.Filter(items, (EngagementRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement record store.</summary>
public static class EngagementRecordStore
{
    public static IEngagementRecordStore NewMemoryEngagementRecordStore()
    {
        return new MemoryEngagementRecordStore(new Dictionary<string, EngagementRecord>(), new List<string>());
    }

    public static IEngagementRecordStore NewCachedEngagementRecordStore(MemoryEngagementRecordStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementRecordStore(inner, EngagementRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EngagementRecord> FindEngagementRecords(IEngagementRecordStore store, EngagementRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
