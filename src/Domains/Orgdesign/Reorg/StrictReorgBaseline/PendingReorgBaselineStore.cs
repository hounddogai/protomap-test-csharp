namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending reorg baselines by code.</summary>
public interface IPendingReorgBaselineStore
{
    PendingReorgBaseline? Find(string code);
    void Save(PendingReorgBaseline item);
    bool Remove(string code);
    List<PendingReorgBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingReorgBaselineStore(Dictionary<string, PendingReorgBaseline> items, List<string> order) : IPendingReorgBaselineStore
{
    public Dictionary<string, PendingReorgBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingReorgBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingReorgBaseline item)
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

    public List<PendingReorgBaseline> List()
    {
        var outValue = new List<PendingReorgBaseline>();
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
public sealed class CachedPendingReorgBaselineStore(MemoryPendingReorgBaselineStore inner, PendingReorgBaselineCache cache, int hits, int misses) : IPendingReorgBaselineStore
{
    public MemoryPendingReorgBaselineStore Inner { get; set; } = inner;
    public PendingReorgBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingReorgBaseline? Find(string code)
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

    public void Save(PendingReorgBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingReorgBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending reorg baselines. Empty fields match all.</summary>
public sealed record PendingReorgBaselineQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PendingReorgBaseline item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<PendingReorgBaseline> Apply(List<PendingReorgBaseline> items)
    {
        var matched = Collections.Filter(items, (PendingReorgBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending reorg baseline store.</summary>
public static class PendingReorgBaselineStore
{
    public static IPendingReorgBaselineStore NewMemoryPendingReorgBaselineStore()
    {
        return new MemoryPendingReorgBaselineStore(
            new Dictionary<string, PendingReorgBaseline>(),
            new List<string>());
    }

    public static IPendingReorgBaselineStore NewCachedPendingReorgBaselineStore(MemoryPendingReorgBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedPendingReorgBaselineStore(inner, PendingReorgBaselineCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingReorgBaseline> FindPendingReorgBaselines(IPendingReorgBaselineStore store, PendingReorgBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
