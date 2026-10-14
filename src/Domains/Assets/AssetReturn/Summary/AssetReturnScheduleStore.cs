namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return schedules by code.</summary>
public interface IAssetReturnScheduleStore
{
    AssetReturnSchedule? Find(string code);
    void Save(AssetReturnSchedule item);
    bool Remove(string code);
    List<AssetReturnSchedule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnScheduleStore(Dictionary<string, AssetReturnSchedule> items, List<string> order) : IAssetReturnScheduleStore
{
    public Dictionary<string, AssetReturnSchedule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnSchedule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnSchedule item)
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

    public List<AssetReturnSchedule> List()
    {
        var outValue = new List<AssetReturnSchedule>();
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
public sealed class CachedAssetReturnScheduleStore(MemoryAssetReturnScheduleStore inner, AssetReturnScheduleCache cache, int hits, int misses) : IAssetReturnScheduleStore
{
    public MemoryAssetReturnScheduleStore Inner { get; set; } = inner;
    public AssetReturnScheduleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnSchedule? Find(string code)
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

    public void Save(AssetReturnSchedule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnSchedule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return schedules. Empty fields match all.</summary>
public sealed record AssetReturnScheduleQuery(string Title, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnSchedule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnSchedule> Apply(List<AssetReturnSchedule> items)
    {
        var matched = Collections.Filter(items, (AssetReturnSchedule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return schedule store.</summary>
public static class AssetReturnScheduleStore
{
    public static IAssetReturnScheduleStore NewMemoryAssetReturnScheduleStore()
    {
        return new MemoryAssetReturnScheduleStore(new Dictionary<string, AssetReturnSchedule>(), new List<string>());
    }

    public static IAssetReturnScheduleStore NewCachedAssetReturnScheduleStore(MemoryAssetReturnScheduleStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnScheduleStore(inner, AssetReturnScheduleCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssetReturnSchedule> FindAssetReturnSchedules(IAssetReturnScheduleStore store, AssetReturnScheduleQuery query)
    {
        return query.Apply(store.List());
    }
}
