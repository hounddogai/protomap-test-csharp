namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy refresh records by code.</summary>
public interface ILegacyRefreshRecordStore
{
    LegacyRefreshRecord? Find(string code);
    void Save(LegacyRefreshRecord item);
    bool Remove(string code);
    List<LegacyRefreshRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyRefreshRecordStore(Dictionary<string, LegacyRefreshRecord> items, List<string> order) : ILegacyRefreshRecordStore
{
    public Dictionary<string, LegacyRefreshRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyRefreshRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyRefreshRecord item)
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

    public List<LegacyRefreshRecord> List()
    {
        var outValue = new List<LegacyRefreshRecord>();
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
public sealed class CachedLegacyRefreshRecordStore(MemoryLegacyRefreshRecordStore inner, LegacyRefreshRecordCache cache, int hits, int misses) : ILegacyRefreshRecordStore
{
    public MemoryLegacyRefreshRecordStore Inner { get; set; } = inner;
    public LegacyRefreshRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyRefreshRecord? Find(string code)
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

    public void Save(LegacyRefreshRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyRefreshRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy refresh records. Empty fields match all.</summary>
public sealed record LegacyRefreshRecordQuery(string Source, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyRefreshRecord item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyRefreshRecord> Apply(List<LegacyRefreshRecord> items)
    {
        var matched = Collections.Filter(items, (LegacyRefreshRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy refresh record store.</summary>
public static class LegacyRefreshRecordStore
{
    public static ILegacyRefreshRecordStore NewMemoryLegacyRefreshRecordStore()
    {
        return new MemoryLegacyRefreshRecordStore(new Dictionary<string, LegacyRefreshRecord>(), new List<string>());
    }

    public static ILegacyRefreshRecordStore NewCachedLegacyRefreshRecordStore(MemoryLegacyRefreshRecordStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyRefreshRecordStore(inner, LegacyRefreshRecordCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<LegacyRefreshRecord> FindLegacyRefreshRecords(ILegacyRefreshRecordStore store, LegacyRefreshRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
