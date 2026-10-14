namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle bundles by code.</summary>
public interface IMeritCycleBundleStore
{
    MeritCycleBundle? Find(string code);
    void Save(MeritCycleBundle item);
    bool Remove(string code);
    List<MeritCycleBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleBundleStore(Dictionary<string, MeritCycleBundle> items, List<string> order) : IMeritCycleBundleStore
{
    public Dictionary<string, MeritCycleBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleBundle item)
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

    public List<MeritCycleBundle> List()
    {
        var outValue = new List<MeritCycleBundle>();
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
public sealed class CachedMeritCycleBundleStore(MemoryMeritCycleBundleStore inner, MeritCycleBundleCache cache, int hits, int misses) : IMeritCycleBundleStore
{
    public MemoryMeritCycleBundleStore Inner { get; set; } = inner;
    public MeritCycleBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleBundle? Find(string code)
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

    public void Save(MeritCycleBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle bundles. Empty fields match all.</summary>
public sealed record MeritCycleBundleQuery(string Department, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleBundle item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleBundle> Apply(List<MeritCycleBundle> items)
    {
        var matched = Collections.Filter(items, (MeritCycleBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle bundle store.</summary>
public static class MeritCycleBundleStore
{
    public static IMeritCycleBundleStore NewMemoryMeritCycleBundleStore()
    {
        return new MemoryMeritCycleBundleStore(new Dictionary<string, MeritCycleBundle>(), new List<string>());
    }

    public static IMeritCycleBundleStore NewCachedMeritCycleBundleStore(MemoryMeritCycleBundleStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleBundleStore(inner, MeritCycleBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleBundle> FindMeritCycleBundles(IMeritCycleBundleStore store, MeritCycleBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
