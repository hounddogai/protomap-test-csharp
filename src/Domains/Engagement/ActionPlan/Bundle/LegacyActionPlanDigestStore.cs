namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy action plan digests by code.</summary>
public interface ILegacyActionPlanDigestStore
{
    LegacyActionPlanDigest? Find(string code);
    void Save(LegacyActionPlanDigest item);
    bool Remove(string code);
    List<LegacyActionPlanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyActionPlanDigestStore(Dictionary<string, LegacyActionPlanDigest> items, List<string> order) : ILegacyActionPlanDigestStore
{
    public Dictionary<string, LegacyActionPlanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyActionPlanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyActionPlanDigest item)
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

    public List<LegacyActionPlanDigest> List()
    {
        var outValue = new List<LegacyActionPlanDigest>();
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
public sealed class CachedLegacyActionPlanDigestStore(MemoryLegacyActionPlanDigestStore inner, LegacyActionPlanDigestCache cache, int hits, int misses) : ILegacyActionPlanDigestStore
{
    public MemoryLegacyActionPlanDigestStore Inner { get; set; } = inner;
    public LegacyActionPlanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyActionPlanDigest? Find(string code)
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

    public void Save(LegacyActionPlanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyActionPlanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy action plan digests. Empty fields match all.</summary>
public sealed record LegacyActionPlanDigestQuery(string Title, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyActionPlanDigest item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyActionPlanDigest> Apply(List<LegacyActionPlanDigest> items)
    {
        var matched = Collections.Filter(items, (LegacyActionPlanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy action plan digest store.</summary>
public static class LegacyActionPlanDigestStore
{
    public static ILegacyActionPlanDigestStore NewMemoryLegacyActionPlanDigestStore()
    {
        return new MemoryLegacyActionPlanDigestStore(
            new Dictionary<string, LegacyActionPlanDigest>(),
            new List<string>());
    }

    public static ILegacyActionPlanDigestStore NewCachedLegacyActionPlanDigestStore(MemoryLegacyActionPlanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyActionPlanDigestStore(
            inner,
            LegacyActionPlanDigestCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyActionPlanDigest> FindLegacyActionPlanDigests(ILegacyActionPlanDigestStore store, LegacyActionPlanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
