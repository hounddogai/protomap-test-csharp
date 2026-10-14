namespace Acme.Workforce.Absence.LeaveRequest.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional leave request baselines by code.</summary>
public interface IRegionalLeaveRequestBaselineStore
{
    RegionalLeaveRequestBaseline? Find(string code);
    void Save(RegionalLeaveRequestBaseline item);
    bool Remove(string code);
    List<RegionalLeaveRequestBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalLeaveRequestBaselineStore(Dictionary<string, RegionalLeaveRequestBaseline> items, List<string> order) : IRegionalLeaveRequestBaselineStore
{
    public Dictionary<string, RegionalLeaveRequestBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalLeaveRequestBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalLeaveRequestBaseline item)
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

    public List<RegionalLeaveRequestBaseline> List()
    {
        var outValue = new List<RegionalLeaveRequestBaseline>();
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
public sealed class CachedRegionalLeaveRequestBaselineStore(MemoryRegionalLeaveRequestBaselineStore inner, RegionalLeaveRequestBaselineCache cache, int hits, int misses) : IRegionalLeaveRequestBaselineStore
{
    public MemoryRegionalLeaveRequestBaselineStore Inner { get; set; } = inner;
    public RegionalLeaveRequestBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalLeaveRequestBaseline? Find(string code)
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

    public void Save(RegionalLeaveRequestBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalLeaveRequestBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional leave request baselines. Empty fields match all.</summary>
public sealed record RegionalLeaveRequestBaselineQuery(string Segment, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalLeaveRequestBaseline item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
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

    public List<RegionalLeaveRequestBaseline> Apply(List<RegionalLeaveRequestBaseline> items)
    {
        var matched = Collections.Filter(items, (RegionalLeaveRequestBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional leave request baseline store.</summary>
public static class RegionalLeaveRequestBaselineStore
{
    public static IRegionalLeaveRequestBaselineStore NewMemoryRegionalLeaveRequestBaselineStore()
    {
        return new MemoryRegionalLeaveRequestBaselineStore(
            new Dictionary<string, RegionalLeaveRequestBaseline>(),
            new List<string>());
    }

    public static IRegionalLeaveRequestBaselineStore NewCachedRegionalLeaveRequestBaselineStore(MemoryRegionalLeaveRequestBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalLeaveRequestBaselineStore(
            inner,
            RegionalLeaveRequestBaselineCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalLeaveRequestBaseline> FindRegionalLeaveRequestBaselines(IRegionalLeaveRequestBaselineStore store, RegionalLeaveRequestBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
