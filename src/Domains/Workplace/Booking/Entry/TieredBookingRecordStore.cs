namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered booking records by code.</summary>
public interface ITieredBookingRecordStore
{
    TieredBookingRecord? Find(string code);
    void Save(TieredBookingRecord item);
    bool Remove(string code);
    List<TieredBookingRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredBookingRecordStore(Dictionary<string, TieredBookingRecord> items, List<string> order) : ITieredBookingRecordStore
{
    public Dictionary<string, TieredBookingRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredBookingRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredBookingRecord item)
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

    public List<TieredBookingRecord> List()
    {
        var outValue = new List<TieredBookingRecord>();
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
public sealed class CachedTieredBookingRecordStore(MemoryTieredBookingRecordStore inner, TieredBookingRecordCache cache, int hits, int misses) : ITieredBookingRecordStore
{
    public MemoryTieredBookingRecordStore Inner { get; set; } = inner;
    public TieredBookingRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredBookingRecord? Find(string code)
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

    public void Save(TieredBookingRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredBookingRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered booking records. Empty fields match all.</summary>
public sealed record TieredBookingRecordQuery(string Locale, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(TieredBookingRecord item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredBookingRecord> Apply(List<TieredBookingRecord> items)
    {
        var matched = Collections.Filter(items, (TieredBookingRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered booking record store.</summary>
public static class TieredBookingRecordStore
{
    public static ITieredBookingRecordStore NewMemoryTieredBookingRecordStore()
    {
        return new MemoryTieredBookingRecordStore(new Dictionary<string, TieredBookingRecord>(), new List<string>());
    }

    public static ITieredBookingRecordStore NewCachedTieredBookingRecordStore(MemoryTieredBookingRecordStore inner, int ttl, IClock clock)
    {
        return new CachedTieredBookingRecordStore(inner, TieredBookingRecordCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<TieredBookingRecord> FindTieredBookingRecords(ITieredBookingRecordStore store, TieredBookingRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
