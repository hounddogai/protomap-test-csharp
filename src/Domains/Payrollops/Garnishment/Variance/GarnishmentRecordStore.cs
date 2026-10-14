namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps garnishment records by code.</summary>
public interface IGarnishmentRecordStore
{
    GarnishmentRecord? Find(string code);
    void Save(GarnishmentRecord item);
    bool Remove(string code);
    List<GarnishmentRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGarnishmentRecordStore(Dictionary<string, GarnishmentRecord> items, List<string> order) : IGarnishmentRecordStore
{
    public Dictionary<string, GarnishmentRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GarnishmentRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GarnishmentRecord item)
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

    public List<GarnishmentRecord> List()
    {
        var outValue = new List<GarnishmentRecord>();
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
public sealed class CachedGarnishmentRecordStore(MemoryGarnishmentRecordStore inner, GarnishmentRecordCache cache, int hits, int misses) : IGarnishmentRecordStore
{
    public MemoryGarnishmentRecordStore Inner { get; set; } = inner;
    public GarnishmentRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GarnishmentRecord? Find(string code)
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

    public void Save(GarnishmentRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GarnishmentRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects garnishment records. Empty fields match all.</summary>
public sealed record GarnishmentRecordQuery(string Category, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(GarnishmentRecord item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GarnishmentRecord> Apply(List<GarnishmentRecord> items)
    {
        var matched = Collections.Filter(items, (GarnishmentRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of garnishment record store.</summary>
public static class GarnishmentRecordStore
{
    public static IGarnishmentRecordStore NewMemoryGarnishmentRecordStore()
    {
        return new MemoryGarnishmentRecordStore(new Dictionary<string, GarnishmentRecord>(), new List<string>());
    }

    public static IGarnishmentRecordStore NewCachedGarnishmentRecordStore(MemoryGarnishmentRecordStore inner, int ttl, IClock clock)
    {
        return new CachedGarnishmentRecordStore(inner, GarnishmentRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<GarnishmentRecord> FindGarnishmentRecords(IGarnishmentRecordStore store, GarnishmentRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
