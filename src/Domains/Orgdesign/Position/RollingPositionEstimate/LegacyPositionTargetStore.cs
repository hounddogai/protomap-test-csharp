namespace Acme.Workforce.Orgdesign.Position.RollingPositionEstimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy position targets by code.</summary>
public interface ILegacyPositionTargetStore
{
    LegacyPositionTarget? Find(string code);
    void Save(LegacyPositionTarget item);
    bool Remove(string code);
    List<LegacyPositionTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyPositionTargetStore(Dictionary<string, LegacyPositionTarget> items, List<string> order) : ILegacyPositionTargetStore
{
    public Dictionary<string, LegacyPositionTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyPositionTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyPositionTarget item)
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

    public List<LegacyPositionTarget> List()
    {
        var outValue = new List<LegacyPositionTarget>();
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
public sealed class CachedLegacyPositionTargetStore(MemoryLegacyPositionTargetStore inner, LegacyPositionTargetCache cache, int hits, int misses) : ILegacyPositionTargetStore
{
    public MemoryLegacyPositionTargetStore Inner { get; set; } = inner;
    public LegacyPositionTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyPositionTarget? Find(string code)
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

    public void Save(LegacyPositionTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyPositionTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy position targets. Empty fields match all.</summary>
public sealed record LegacyPositionTargetQuery(string Unit, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyPositionTarget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyPositionTarget> Apply(List<LegacyPositionTarget> items)
    {
        var matched = Collections.Filter(items, (LegacyPositionTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy position target store.</summary>
public static class LegacyPositionTargetStore
{
    public static ILegacyPositionTargetStore NewMemoryLegacyPositionTargetStore()
    {
        return new MemoryLegacyPositionTargetStore(
            new Dictionary<string, LegacyPositionTarget>(),
            new List<string>());
    }

    public static ILegacyPositionTargetStore NewCachedLegacyPositionTargetStore(MemoryLegacyPositionTargetStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyPositionTargetStore(inner, LegacyPositionTargetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<LegacyPositionTarget> FindLegacyPositionTargets(ILegacyPositionTargetStore store, LegacyPositionTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
