namespace Acme.Workforce.Compensation.MeritCycle.CycleRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle policies by code.</summary>
public interface IMeritCyclePolicyStore
{
    MeritCyclePolicy? Find(string code);
    void Save(MeritCyclePolicy item);
    bool Remove(string code);
    List<MeritCyclePolicy> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCyclePolicyStore(Dictionary<string, MeritCyclePolicy> items, List<string> order) : IMeritCyclePolicyStore
{
    public Dictionary<string, MeritCyclePolicy> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCyclePolicy? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCyclePolicy item)
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

    public List<MeritCyclePolicy> List()
    {
        var outValue = new List<MeritCyclePolicy>();
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
public sealed class CachedMeritCyclePolicyStore(MemoryMeritCyclePolicyStore inner, MeritCyclePolicyCache cache, int hits, int misses) : IMeritCyclePolicyStore
{
    public MemoryMeritCyclePolicyStore Inner { get; set; } = inner;
    public MeritCyclePolicyCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCyclePolicy? Find(string code)
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

    public void Save(MeritCyclePolicy item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCyclePolicy> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle policies. Empty fields match all.</summary>
public sealed record MeritCyclePolicyQuery(string Label, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCyclePolicy item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCyclePolicy> Apply(List<MeritCyclePolicy> items)
    {
        var matched = Collections.Filter(items, (MeritCyclePolicy candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle policy store.</summary>
public static class MeritCyclePolicyStore
{
    public static IMeritCyclePolicyStore NewMemoryMeritCyclePolicyStore()
    {
        return new MemoryMeritCyclePolicyStore(new Dictionary<string, MeritCyclePolicy>(), new List<string>());
    }

    public static IMeritCyclePolicyStore NewCachedMeritCyclePolicyStore(MemoryMeritCyclePolicyStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCyclePolicyStore(inner, MeritCyclePolicyCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCyclePolicy> FindMeritCyclePolicies(IMeritCyclePolicyStore store, MeritCyclePolicyQuery query)
    {
        return query.Apply(store.List());
    }
}
