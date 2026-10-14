namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default mobility targets by code.</summary>
public interface IDefaultMobilityTargetStore
{
    DefaultMobilityTarget? Find(string code);
    void Save(DefaultMobilityTarget item);
    bool Remove(string code);
    List<DefaultMobilityTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultMobilityTargetStore(Dictionary<string, DefaultMobilityTarget> items, List<string> order) : IDefaultMobilityTargetStore
{
    public Dictionary<string, DefaultMobilityTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultMobilityTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultMobilityTarget item)
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

    public List<DefaultMobilityTarget> List()
    {
        var outValue = new List<DefaultMobilityTarget>();
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
public sealed class CachedDefaultMobilityTargetStore(MemoryDefaultMobilityTargetStore inner, DefaultMobilityTargetCache cache, int hits, int misses) : IDefaultMobilityTargetStore
{
    public MemoryDefaultMobilityTargetStore Inner { get; set; } = inner;
    public DefaultMobilityTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultMobilityTarget? Find(string code)
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

    public void Save(DefaultMobilityTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultMobilityTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default mobility targets. Empty fields match all.</summary>
public sealed record DefaultMobilityTargetQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultMobilityTarget item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultMobilityTarget> Apply(List<DefaultMobilityTarget> items)
    {
        var matched = Collections.Filter(items, (DefaultMobilityTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default mobility target store.</summary>
public static class DefaultMobilityTargetStore
{
    public static IDefaultMobilityTargetStore NewMemoryDefaultMobilityTargetStore()
    {
        return new MemoryDefaultMobilityTargetStore(
            new Dictionary<string, DefaultMobilityTarget>(),
            new List<string>());
    }

    public static IDefaultMobilityTargetStore NewCachedDefaultMobilityTargetStore(MemoryDefaultMobilityTargetStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultMobilityTargetStore(inner, DefaultMobilityTargetCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<DefaultMobilityTarget> FindDefaultMobilityTargets(IDefaultMobilityTargetStore store, DefaultMobilityTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
