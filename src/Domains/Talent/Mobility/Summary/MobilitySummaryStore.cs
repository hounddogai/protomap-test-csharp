namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility summaries by code.</summary>
public interface IMobilitySummaryStore
{
    MobilitySummary? Find(string code);
    void Save(MobilitySummary item);
    bool Remove(string code);
    List<MobilitySummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilitySummaryStore(Dictionary<string, MobilitySummary> items, List<string> order) : IMobilitySummaryStore
{
    public Dictionary<string, MobilitySummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilitySummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilitySummary item)
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

    public List<MobilitySummary> List()
    {
        var outValue = new List<MobilitySummary>();
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
public sealed class CachedMobilitySummaryStore(MemoryMobilitySummaryStore inner, MobilitySummaryCache cache, int hits, int misses) : IMobilitySummaryStore
{
    public MemoryMobilitySummaryStore Inner { get; set; } = inner;
    public MobilitySummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilitySummary? Find(string code)
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

    public void Save(MobilitySummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilitySummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility summaries. Empty fields match all.</summary>
public sealed record MobilitySummaryQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(MobilitySummary item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MobilitySummary> Apply(List<MobilitySummary> items)
    {
        var matched = Collections.Filter(items, (MobilitySummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility summary store.</summary>
public static class MobilitySummaryStore
{
    public static IMobilitySummaryStore NewMemoryMobilitySummaryStore()
    {
        return new MemoryMobilitySummaryStore(new Dictionary<string, MobilitySummary>(), new List<string>());
    }

    public static IMobilitySummaryStore NewCachedMobilitySummaryStore(MemoryMobilitySummaryStore inner, int ttl, IClock clock)
    {
        return new CachedMobilitySummaryStore(inner, MobilitySummaryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<MobilitySummary> FindMobilitySummaries(IMobilitySummaryStore store, MobilitySummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
