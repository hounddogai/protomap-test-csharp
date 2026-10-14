namespace Acme.Workforce.Talent.Mobility.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary mobility targets by code.</summary>
public interface IPrimaryMobilityTargetStore
{
    PrimaryMobilityTarget? Find(string code);
    void Save(PrimaryMobilityTarget item);
    bool Remove(string code);
    List<PrimaryMobilityTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryMobilityTargetStore(Dictionary<string, PrimaryMobilityTarget> items, List<string> order) : IPrimaryMobilityTargetStore
{
    public Dictionary<string, PrimaryMobilityTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryMobilityTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryMobilityTarget item)
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

    public List<PrimaryMobilityTarget> List()
    {
        var outValue = new List<PrimaryMobilityTarget>();
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
public sealed class CachedPrimaryMobilityTargetStore(MemoryPrimaryMobilityTargetStore inner, PrimaryMobilityTargetCache cache, int hits, int misses) : IPrimaryMobilityTargetStore
{
    public MemoryPrimaryMobilityTargetStore Inner { get; set; } = inner;
    public PrimaryMobilityTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryMobilityTarget? Find(string code)
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

    public void Save(PrimaryMobilityTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryMobilityTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary mobility targets. Empty fields match all.</summary>
public sealed record PrimaryMobilityTargetQuery(string Channel, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryMobilityTarget item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryMobilityTarget> Apply(List<PrimaryMobilityTarget> items)
    {
        var matched = Collections.Filter(items, (PrimaryMobilityTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary mobility target store.</summary>
public static class PrimaryMobilityTargetStore
{
    public static IPrimaryMobilityTargetStore NewMemoryPrimaryMobilityTargetStore()
    {
        return new MemoryPrimaryMobilityTargetStore(
            new Dictionary<string, PrimaryMobilityTarget>(),
            new List<string>());
    }

    public static IPrimaryMobilityTargetStore NewCachedPrimaryMobilityTargetStore(MemoryPrimaryMobilityTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryMobilityTargetStore(
            inner,
            PrimaryMobilityTargetCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryMobilityTarget> FindPrimaryMobilityTargets(IPrimaryMobilityTargetStore store, PrimaryMobilityTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
