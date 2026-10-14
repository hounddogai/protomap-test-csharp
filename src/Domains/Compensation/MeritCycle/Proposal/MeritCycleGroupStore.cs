namespace Acme.Workforce.Compensation.MeritCycle.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps merit cycle groups by code.</summary>
public interface IMeritCycleGroupStore
{
    MeritCycleGroup? Find(string code);
    void Save(MeritCycleGroup item);
    bool Remove(string code);
    List<MeritCycleGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMeritCycleGroupStore(Dictionary<string, MeritCycleGroup> items, List<string> order) : IMeritCycleGroupStore
{
    public Dictionary<string, MeritCycleGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MeritCycleGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MeritCycleGroup item)
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

    public List<MeritCycleGroup> List()
    {
        var outValue = new List<MeritCycleGroup>();
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
public sealed class CachedMeritCycleGroupStore(MemoryMeritCycleGroupStore inner, MeritCycleGroupCache cache, int hits, int misses) : IMeritCycleGroupStore
{
    public MemoryMeritCycleGroupStore Inner { get; set; } = inner;
    public MeritCycleGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MeritCycleGroup? Find(string code)
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

    public void Save(MeritCycleGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MeritCycleGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects merit cycle groups. Empty fields match all.</summary>
public sealed record MeritCycleGroupQuery(string Category, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(MeritCycleGroup item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<MeritCycleGroup> Apply(List<MeritCycleGroup> items)
    {
        var matched = Collections.Filter(items, (MeritCycleGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of merit cycle group store.</summary>
public static class MeritCycleGroupStore
{
    public static IMeritCycleGroupStore NewMemoryMeritCycleGroupStore()
    {
        return new MemoryMeritCycleGroupStore(new Dictionary<string, MeritCycleGroup>(), new List<string>());
    }

    public static IMeritCycleGroupStore NewCachedMeritCycleGroupStore(MemoryMeritCycleGroupStore inner, int ttl, IClock clock)
    {
        return new CachedMeritCycleGroupStore(inner, MeritCycleGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<MeritCycleGroup> FindMeritCycleGroups(IMeritCycleGroupStore store, MeritCycleGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
