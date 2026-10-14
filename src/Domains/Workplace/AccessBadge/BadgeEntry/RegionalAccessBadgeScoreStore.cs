namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional access badge scores by code.</summary>
public interface IRegionalAccessBadgeScoreStore
{
    RegionalAccessBadgeScore? Find(string code);
    void Save(RegionalAccessBadgeScore item);
    bool Remove(string code);
    List<RegionalAccessBadgeScore> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAccessBadgeScoreStore(Dictionary<string, RegionalAccessBadgeScore> items, List<string> order) : IRegionalAccessBadgeScoreStore
{
    public Dictionary<string, RegionalAccessBadgeScore> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAccessBadgeScore? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAccessBadgeScore item)
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

    public List<RegionalAccessBadgeScore> List()
    {
        var outValue = new List<RegionalAccessBadgeScore>();
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
public sealed class CachedRegionalAccessBadgeScoreStore(MemoryRegionalAccessBadgeScoreStore inner, RegionalAccessBadgeScoreCache cache, int hits, int misses) : IRegionalAccessBadgeScoreStore
{
    public MemoryRegionalAccessBadgeScoreStore Inner { get; set; } = inner;
    public RegionalAccessBadgeScoreCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAccessBadgeScore? Find(string code)
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

    public void Save(RegionalAccessBadgeScore item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAccessBadgeScore> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional access badge scores. Empty fields match all.</summary>
public sealed record RegionalAccessBadgeScoreQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAccessBadgeScore item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAccessBadgeScore> Apply(List<RegionalAccessBadgeScore> items)
    {
        var matched = Collections.Filter(items, (RegionalAccessBadgeScore candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional access badge score store.</summary>
public static class RegionalAccessBadgeScoreStore
{
    public static IRegionalAccessBadgeScoreStore NewMemoryRegionalAccessBadgeScoreStore()
    {
        return new MemoryRegionalAccessBadgeScoreStore(
            new Dictionary<string, RegionalAccessBadgeScore>(),
            new List<string>());
    }

    public static IRegionalAccessBadgeScoreStore NewCachedRegionalAccessBadgeScoreStore(MemoryRegionalAccessBadgeScoreStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAccessBadgeScoreStore(
            inner,
            RegionalAccessBadgeScoreCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAccessBadgeScore> FindRegionalAccessBadgeScores(IRegionalAccessBadgeScoreStore store, RegionalAccessBadgeScoreQuery query)
    {
        return query.Apply(store.List());
    }
}
