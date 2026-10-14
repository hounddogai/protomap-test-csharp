namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled span digests by code.</summary>
public interface IScheduledSpanDigestStore
{
    ScheduledSpanDigest? Find(string code);
    void Save(ScheduledSpanDigest item);
    bool Remove(string code);
    List<ScheduledSpanDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledSpanDigestStore(Dictionary<string, ScheduledSpanDigest> items, List<string> order) : IScheduledSpanDigestStore
{
    public Dictionary<string, ScheduledSpanDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledSpanDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledSpanDigest item)
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

    public List<ScheduledSpanDigest> List()
    {
        var outValue = new List<ScheduledSpanDigest>();
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
public sealed class CachedScheduledSpanDigestStore(MemoryScheduledSpanDigestStore inner, ScheduledSpanDigestCache cache, int hits, int misses) : IScheduledSpanDigestStore
{
    public MemoryScheduledSpanDigestStore Inner { get; set; } = inner;
    public ScheduledSpanDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledSpanDigest? Find(string code)
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

    public void Save(ScheduledSpanDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledSpanDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled span digests. Empty fields match all.</summary>
public sealed record ScheduledSpanDigestQuery(string Unit, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledSpanDigest item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledSpanDigest> Apply(List<ScheduledSpanDigest> items)
    {
        var matched = Collections.Filter(items, (ScheduledSpanDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled span digest store.</summary>
public static class ScheduledSpanDigestStore
{
    public static IScheduledSpanDigestStore NewMemoryScheduledSpanDigestStore()
    {
        return new MemoryScheduledSpanDigestStore(new Dictionary<string, ScheduledSpanDigest>(), new List<string>());
    }

    public static IScheduledSpanDigestStore NewCachedScheduledSpanDigestStore(MemoryScheduledSpanDigestStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledSpanDigestStore(inner, ScheduledSpanDigestCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ScheduledSpanDigest> FindScheduledSpanDigests(IScheduledSpanDigestStore store, ScheduledSpanDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
