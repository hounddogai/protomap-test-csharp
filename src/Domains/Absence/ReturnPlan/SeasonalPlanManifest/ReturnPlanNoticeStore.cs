namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps return plan notices by code.</summary>
public interface IReturnPlanNoticeStore
{
    ReturnPlanNotice? Find(string code);
    void Save(ReturnPlanNotice item);
    bool Remove(string code);
    List<ReturnPlanNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReturnPlanNoticeStore(Dictionary<string, ReturnPlanNotice> items, List<string> order) : IReturnPlanNoticeStore
{
    public Dictionary<string, ReturnPlanNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReturnPlanNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReturnPlanNotice item)
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

    public List<ReturnPlanNotice> List()
    {
        var outValue = new List<ReturnPlanNotice>();
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
public sealed class CachedReturnPlanNoticeStore(MemoryReturnPlanNoticeStore inner, ReturnPlanNoticeCache cache, int hits, int misses) : IReturnPlanNoticeStore
{
    public MemoryReturnPlanNoticeStore Inner { get; set; } = inner;
    public ReturnPlanNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReturnPlanNotice? Find(string code)
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

    public void Save(ReturnPlanNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReturnPlanNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects return plan notices. Empty fields match all.</summary>
public sealed record ReturnPlanNoticeQuery(string Team, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(ReturnPlanNotice item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReturnPlanNotice> Apply(List<ReturnPlanNotice> items)
    {
        var matched = Collections.Filter(items, (ReturnPlanNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of return plan notice store.</summary>
public static class ReturnPlanNoticeStore
{
    public static IReturnPlanNoticeStore NewMemoryReturnPlanNoticeStore()
    {
        return new MemoryReturnPlanNoticeStore(new Dictionary<string, ReturnPlanNotice>(), new List<string>());
    }

    public static IReturnPlanNoticeStore NewCachedReturnPlanNoticeStore(MemoryReturnPlanNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedReturnPlanNoticeStore(inner, ReturnPlanNoticeCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<ReturnPlanNotice> FindReturnPlanNotices(IReturnPlanNoticeStore store, ReturnPlanNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
