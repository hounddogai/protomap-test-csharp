namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped leave request baselines by code.</summary>
public interface ICappedLeaveRequestBaselineStore
{
    CappedLeaveRequestBaseline? Find(string code);
    void Save(CappedLeaveRequestBaseline item);
    bool Remove(string code);
    List<CappedLeaveRequestBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedLeaveRequestBaselineStore(Dictionary<string, CappedLeaveRequestBaseline> items, List<string> order) : ICappedLeaveRequestBaselineStore
{
    public Dictionary<string, CappedLeaveRequestBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedLeaveRequestBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedLeaveRequestBaseline item)
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

    public List<CappedLeaveRequestBaseline> List()
    {
        var outValue = new List<CappedLeaveRequestBaseline>();
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
public sealed class CachedCappedLeaveRequestBaselineStore(MemoryCappedLeaveRequestBaselineStore inner, CappedLeaveRequestBaselineCache cache, int hits, int misses) : ICappedLeaveRequestBaselineStore
{
    public MemoryCappedLeaveRequestBaselineStore Inner { get; set; } = inner;
    public CappedLeaveRequestBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedLeaveRequestBaseline? Find(string code)
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

    public void Save(CappedLeaveRequestBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedLeaveRequestBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped leave request baselines. Empty fields match all.</summary>
public sealed record CappedLeaveRequestBaselineQuery(string Country, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(CappedLeaveRequestBaseline item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<CappedLeaveRequestBaseline> Apply(List<CappedLeaveRequestBaseline> items)
    {
        var matched = Collections.Filter(items, (CappedLeaveRequestBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped leave request baseline store.</summary>
public static class CappedLeaveRequestBaselineStore
{
    public static ICappedLeaveRequestBaselineStore NewMemoryCappedLeaveRequestBaselineStore()
    {
        return new MemoryCappedLeaveRequestBaselineStore(
            new Dictionary<string, CappedLeaveRequestBaseline>(),
            new List<string>());
    }

    public static ICappedLeaveRequestBaselineStore NewCachedCappedLeaveRequestBaselineStore(MemoryCappedLeaveRequestBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedCappedLeaveRequestBaselineStore(
            inner,
            CappedLeaveRequestBaselineCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<CappedLeaveRequestBaseline> FindCappedLeaveRequestBaselines(ICappedLeaveRequestBaselineStore store, CappedLeaveRequestBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
