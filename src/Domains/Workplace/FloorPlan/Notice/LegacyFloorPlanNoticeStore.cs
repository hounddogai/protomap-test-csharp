namespace Acme.Workforce.Workplace.FloorPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy floor plan notices by code.</summary>
public interface ILegacyFloorPlanNoticeStore
{
    LegacyFloorPlanNotice? Find(string code);
    void Save(LegacyFloorPlanNotice item);
    bool Remove(string code);
    List<LegacyFloorPlanNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyFloorPlanNoticeStore(Dictionary<string, LegacyFloorPlanNotice> items, List<string> order) : ILegacyFloorPlanNoticeStore
{
    public Dictionary<string, LegacyFloorPlanNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyFloorPlanNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyFloorPlanNotice item)
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

    public List<LegacyFloorPlanNotice> List()
    {
        var outValue = new List<LegacyFloorPlanNotice>();
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
public sealed class CachedLegacyFloorPlanNoticeStore(MemoryLegacyFloorPlanNoticeStore inner, LegacyFloorPlanNoticeCache cache, int hits, int misses) : ILegacyFloorPlanNoticeStore
{
    public MemoryLegacyFloorPlanNoticeStore Inner { get; set; } = inner;
    public LegacyFloorPlanNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyFloorPlanNotice? Find(string code)
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

    public void Save(LegacyFloorPlanNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyFloorPlanNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy floor plan notices. Empty fields match all.</summary>
public sealed record LegacyFloorPlanNoticeQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyFloorPlanNotice item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<LegacyFloorPlanNotice> Apply(List<LegacyFloorPlanNotice> items)
    {
        var matched = Collections.Filter(items, (LegacyFloorPlanNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy floor plan notice store.</summary>
public static class LegacyFloorPlanNoticeStore
{
    public static ILegacyFloorPlanNoticeStore NewMemoryLegacyFloorPlanNoticeStore()
    {
        return new MemoryLegacyFloorPlanNoticeStore(
            new Dictionary<string, LegacyFloorPlanNotice>(),
            new List<string>());
    }

    public static ILegacyFloorPlanNoticeStore NewCachedLegacyFloorPlanNoticeStore(MemoryLegacyFloorPlanNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyFloorPlanNoticeStore(inner, LegacyFloorPlanNoticeCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<LegacyFloorPlanNotice> FindLegacyFloorPlanNotices(ILegacyFloorPlanNoticeStore store, LegacyFloorPlanNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
