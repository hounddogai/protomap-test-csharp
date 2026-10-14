namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility targets by code.</summary>
public interface IMobilityTargetStore
{
    MobilityTarget? Find(string code);
    void Save(MobilityTarget item);
    bool Remove(string code);
    List<MobilityTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityTargetStore(Dictionary<string, MobilityTarget> items, List<string> order) : IMobilityTargetStore
{
    public Dictionary<string, MobilityTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityTarget item)
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

    public List<MobilityTarget> List()
    {
        var outValue = new List<MobilityTarget>();
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
public sealed class CachedMobilityTargetStore(MemoryMobilityTargetStore inner, MobilityTargetCache cache, int hits, int misses) : IMobilityTargetStore
{
    public MemoryMobilityTargetStore Inner { get; set; } = inner;
    public MobilityTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityTarget? Find(string code)
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

    public void Save(MobilityTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility targets. Empty fields match all.</summary>
public sealed record MobilityTargetQuery(string Locale, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityTarget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<MobilityTarget> Apply(List<MobilityTarget> items)
    {
        var matched = Collections.Filter(items, (MobilityTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility target store.</summary>
public static class MobilityTargetStore
{
    public static IMobilityTargetStore NewMemoryMobilityTargetStore()
    {
        return new MemoryMobilityTargetStore(new Dictionary<string, MobilityTarget>(), new List<string>());
    }

    public static IMobilityTargetStore NewCachedMobilityTargetStore(MemoryMobilityTargetStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityTargetStore(inner, MobilityTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<MobilityTarget> FindMobilityTargets(IMobilityTargetStore store, MobilityTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
