namespace Acme.Workforce.Assets.AssetReturn.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global asset return records by code.</summary>
public interface IGlobalAssetReturnRecordStore
{
    GlobalAssetReturnRecord? Find(string code);
    void Save(GlobalAssetReturnRecord item);
    bool Remove(string code);
    List<GlobalAssetReturnRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalAssetReturnRecordStore(Dictionary<string, GlobalAssetReturnRecord> items, List<string> order) : IGlobalAssetReturnRecordStore
{
    public Dictionary<string, GlobalAssetReturnRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalAssetReturnRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalAssetReturnRecord item)
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

    public List<GlobalAssetReturnRecord> List()
    {
        var outValue = new List<GlobalAssetReturnRecord>();
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
public sealed class CachedGlobalAssetReturnRecordStore(MemoryGlobalAssetReturnRecordStore inner, GlobalAssetReturnRecordCache cache, int hits, int misses) : IGlobalAssetReturnRecordStore
{
    public MemoryGlobalAssetReturnRecordStore Inner { get; set; } = inner;
    public GlobalAssetReturnRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalAssetReturnRecord? Find(string code)
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

    public void Save(GlobalAssetReturnRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalAssetReturnRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global asset return records. Empty fields match all.</summary>
public sealed record GlobalAssetReturnRecordQuery(string Segment, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalAssetReturnRecord item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalAssetReturnRecord> Apply(List<GlobalAssetReturnRecord> items)
    {
        var matched = Collections.Filter(items, (GlobalAssetReturnRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global asset return record store.</summary>
public static class GlobalAssetReturnRecordStore
{
    public static IGlobalAssetReturnRecordStore NewMemoryGlobalAssetReturnRecordStore()
    {
        return new MemoryGlobalAssetReturnRecordStore(
            new Dictionary<string, GlobalAssetReturnRecord>(),
            new List<string>());
    }

    public static IGlobalAssetReturnRecordStore NewCachedGlobalAssetReturnRecordStore(MemoryGlobalAssetReturnRecordStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalAssetReturnRecordStore(
            inner,
            GlobalAssetReturnRecordCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<GlobalAssetReturnRecord> FindGlobalAssetReturnRecords(IGlobalAssetReturnRecordStore store, GlobalAssetReturnRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
