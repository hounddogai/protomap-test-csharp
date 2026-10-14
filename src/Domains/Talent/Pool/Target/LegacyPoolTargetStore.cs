namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy pool targets by code.</summary>
public interface ILegacyPoolTargetStore
{
    LegacyPoolTarget? Find(string code);
    void Save(LegacyPoolTarget item);
    bool Remove(string code);
    List<LegacyPoolTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPoolTargetStore(Dictionary<string, LegacyPoolTarget> items, List<string> order) : ILegacyPoolTargetStore
{
    public Dictionary<string, LegacyPoolTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPoolTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPoolTarget item)
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

    public List<LegacyPoolTarget> List()
    {
        var outValue = new List<LegacyPoolTarget>();
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
public sealed class CachedLegacyPoolTargetStore(MemoryLegacyPoolTargetStore inner, LegacyPoolTargetCache cache, int hits, int misses) : ILegacyPoolTargetStore
{
    public MemoryLegacyPoolTargetStore Inner { get; set; } = inner;
    public LegacyPoolTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPoolTarget? Find(string code)
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

    public void Save(LegacyPoolTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPoolTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy pool targets. Empty fields match all.</summary>
public sealed record LegacyPoolTargetQuery(string Name, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPoolTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<LegacyPoolTarget> Apply(List<LegacyPoolTarget> items)
    {
        var matched = Collections.Filter(items, (LegacyPoolTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy pool target store.</summary>
public static class LegacyPoolTargetStore
{
    public static ILegacyPoolTargetStore NewMemoryLegacyPoolTargetStore()
    {
        return new MemoryLegacyPoolTargetStore(new Dictionary<string, LegacyPoolTarget>(), new List<string>());
    }

    public static ILegacyPoolTargetStore NewCachedLegacyPoolTargetStore(MemoryLegacyPoolTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPoolTargetStore(inner, LegacyPoolTargetCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<LegacyPoolTarget> FindLegacyPoolTargets(ILegacyPoolTargetStore store, LegacyPoolTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
