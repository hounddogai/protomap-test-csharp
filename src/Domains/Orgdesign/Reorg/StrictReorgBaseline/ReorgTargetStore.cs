namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg targets by code.</summary>
public interface IReorgTargetStore
{
    ReorgTarget? Find(string code);
    void Save(ReorgTarget item);
    bool Remove(string code);
    List<ReorgTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgTargetStore(Dictionary<string, ReorgTarget> items, List<string> order) : IReorgTargetStore
{
    public Dictionary<string, ReorgTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgTarget item)
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

    public List<ReorgTarget> List()
    {
        var outValue = new List<ReorgTarget>();
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
public sealed class CachedReorgTargetStore(MemoryReorgTargetStore inner, ReorgTargetCache cache, int hits, int misses) : IReorgTargetStore
{
    public MemoryReorgTargetStore Inner { get; set; } = inner;
    public ReorgTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgTarget? Find(string code)
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

    public void Save(ReorgTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg targets. Empty fields match all.</summary>
public sealed record ReorgTargetQuery(string Locale, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgTarget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ReorgTarget> Apply(List<ReorgTarget> items)
    {
        var matched = Collections.Filter(items, (ReorgTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg target store.</summary>
public static class ReorgTargetStore
{
    public static IReorgTargetStore NewMemoryReorgTargetStore()
    {
        return new MemoryReorgTargetStore(new Dictionary<string, ReorgTarget>(), new List<string>());
    }

    public static IReorgTargetStore NewCachedReorgTargetStore(MemoryReorgTargetStore inner, int ttl, IClock clock)
    {
        return new CachedReorgTargetStore(inner, ReorgTargetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ReorgTarget> FindReorgTargets(IReorgTargetStore store, ReorgTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
