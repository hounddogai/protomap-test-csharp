namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted audit records by code.</summary>
public interface IWeightedAuditRecordStore
{
    WeightedAuditRecord? Find(string code);
    void Save(WeightedAuditRecord item);
    bool Remove(string code);
    List<WeightedAuditRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAuditRecordStore(Dictionary<string, WeightedAuditRecord> items, List<string> order) : IWeightedAuditRecordStore
{
    public Dictionary<string, WeightedAuditRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAuditRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAuditRecord item)
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

    public List<WeightedAuditRecord> List()
    {
        var outValue = new List<WeightedAuditRecord>();
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
public sealed class CachedWeightedAuditRecordStore(MemoryWeightedAuditRecordStore inner, WeightedAuditRecordCache cache, int hits, int misses) : IWeightedAuditRecordStore
{
    public MemoryWeightedAuditRecordStore Inner { get; set; } = inner;
    public WeightedAuditRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAuditRecord? Find(string code)
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

    public void Save(WeightedAuditRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAuditRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted audit records. Empty fields match all.</summary>
public sealed record WeightedAuditRecordQuery(string Team, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAuditRecord item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAuditRecord> Apply(List<WeightedAuditRecord> items)
    {
        var matched = Collections.Filter(items, (WeightedAuditRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted audit record store.</summary>
public static class WeightedAuditRecordStore
{
    public static IWeightedAuditRecordStore NewMemoryWeightedAuditRecordStore()
    {
        return new MemoryWeightedAuditRecordStore(new Dictionary<string, WeightedAuditRecord>(), new List<string>());
    }

    public static IWeightedAuditRecordStore NewCachedWeightedAuditRecordStore(MemoryWeightedAuditRecordStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAuditRecordStore(inner, WeightedAuditRecordCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<WeightedAuditRecord> FindWeightedAuditRecords(IWeightedAuditRecordStore store, WeightedAuditRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
