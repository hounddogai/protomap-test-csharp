namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary floor plan notices by code.</summary>
public interface IPrimaryFloorPlanNoticeStore
{
    PrimaryFloorPlanNotice? Find(string code);
    void Save(PrimaryFloorPlanNotice item);
    bool Remove(string code);
    List<PrimaryFloorPlanNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryFloorPlanNoticeStore(Dictionary<string, PrimaryFloorPlanNotice> items, List<string> order) : IPrimaryFloorPlanNoticeStore
{
    public Dictionary<string, PrimaryFloorPlanNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryFloorPlanNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryFloorPlanNotice item)
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

    public List<PrimaryFloorPlanNotice> List()
    {
        var outValue = new List<PrimaryFloorPlanNotice>();
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
public sealed class CachedPrimaryFloorPlanNoticeStore(MemoryPrimaryFloorPlanNoticeStore inner, PrimaryFloorPlanNoticeCache cache, int hits, int misses) : IPrimaryFloorPlanNoticeStore
{
    public MemoryPrimaryFloorPlanNoticeStore Inner { get; set; } = inner;
    public PrimaryFloorPlanNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryFloorPlanNotice? Find(string code)
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

    public void Save(PrimaryFloorPlanNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryFloorPlanNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary floor plan notices. Empty fields match all.</summary>
public sealed record PrimaryFloorPlanNoticeQuery(string Label, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryFloorPlanNotice item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryFloorPlanNotice> Apply(List<PrimaryFloorPlanNotice> items)
    {
        var matched = Collections.Filter(items, (PrimaryFloorPlanNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary floor plan notice store.</summary>
public static class PrimaryFloorPlanNoticeStore
{
    public static IPrimaryFloorPlanNoticeStore NewMemoryPrimaryFloorPlanNoticeStore()
    {
        return new MemoryPrimaryFloorPlanNoticeStore(
            new Dictionary<string, PrimaryFloorPlanNotice>(),
            new List<string>());
    }

    public static IPrimaryFloorPlanNoticeStore NewCachedPrimaryFloorPlanNoticeStore(MemoryPrimaryFloorPlanNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryFloorPlanNoticeStore(
            inner,
            PrimaryFloorPlanNoticeCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryFloorPlanNotice> FindPrimaryFloorPlanNotices(IPrimaryFloorPlanNoticeStore store, PrimaryFloorPlanNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
