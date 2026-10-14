namespace Acme.Workforce.Workplace.Occupancy.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps occupancy notices by code.</summary>
public interface IOccupancyNoticeStore
{
    OccupancyNotice? Find(string code);
    void Save(OccupancyNotice item);
    bool Remove(string code);
    List<OccupancyNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOccupancyNoticeStore(Dictionary<string, OccupancyNotice> items, List<string> order) : IOccupancyNoticeStore
{
    public Dictionary<string, OccupancyNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OccupancyNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OccupancyNotice item)
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

    public List<OccupancyNotice> List()
    {
        var outValue = new List<OccupancyNotice>();
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
public sealed class CachedOccupancyNoticeStore(MemoryOccupancyNoticeStore inner, OccupancyNoticeCache cache, int hits, int misses) : IOccupancyNoticeStore
{
    public MemoryOccupancyNoticeStore Inner { get; set; } = inner;
    public OccupancyNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OccupancyNotice? Find(string code)
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

    public void Save(OccupancyNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OccupancyNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects occupancy notices. Empty fields match all.</summary>
public sealed record OccupancyNoticeQuery(string Department, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(OccupancyNotice item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
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

    public List<OccupancyNotice> Apply(List<OccupancyNotice> items)
    {
        var matched = Collections.Filter(items, (OccupancyNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of occupancy notice store.</summary>
public static class OccupancyNoticeStore
{
    public static IOccupancyNoticeStore NewMemoryOccupancyNoticeStore()
    {
        return new MemoryOccupancyNoticeStore(new Dictionary<string, OccupancyNotice>(), new List<string>());
    }

    public static IOccupancyNoticeStore NewCachedOccupancyNoticeStore(MemoryOccupancyNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedOccupancyNoticeStore(inner, OccupancyNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<OccupancyNotice> FindOccupancyNotices(IOccupancyNoticeStore store, OccupancyNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
