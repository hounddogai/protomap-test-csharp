namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps holiday batches by code.</summary>
public interface IHolidayBatchStore
{
    HolidayBatch? Find(string code);
    void Save(HolidayBatch item);
    bool Remove(string code);
    List<HolidayBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHolidayBatchStore(Dictionary<string, HolidayBatch> items, List<string> order) : IHolidayBatchStore
{
    public Dictionary<string, HolidayBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HolidayBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HolidayBatch item)
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

    public List<HolidayBatch> List()
    {
        var outValue = new List<HolidayBatch>();
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
public sealed class CachedHolidayBatchStore(MemoryHolidayBatchStore inner, HolidayBatchCache cache, int hits, int misses) : IHolidayBatchStore
{
    public MemoryHolidayBatchStore Inner { get; set; } = inner;
    public HolidayBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HolidayBatch? Find(string code)
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

    public void Save(HolidayBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HolidayBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects holiday batches. Empty fields match all.</summary>
public sealed record HolidayBatchQuery(string Channel, double MinThreshold, List<string> Statuses, int Limit)
{
    public bool Matches(HolidayBatch item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Threshold < MinThreshold)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HolidayBatch> Apply(List<HolidayBatch> items)
    {
        var matched = Collections.Filter(items, (HolidayBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of holiday batch store.</summary>
public static class HolidayBatchStore
{
    public static IHolidayBatchStore NewMemoryHolidayBatchStore()
    {
        return new MemoryHolidayBatchStore(new Dictionary<string, HolidayBatch>(), new List<string>());
    }

    public static IHolidayBatchStore NewCachedHolidayBatchStore(MemoryHolidayBatchStore inner, int ttl, IClock clock)
    {
        return new CachedHolidayBatchStore(inner, HolidayBatchCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HolidayBatch> FindHolidayBatches(IHolidayBatchStore store, HolidayBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
