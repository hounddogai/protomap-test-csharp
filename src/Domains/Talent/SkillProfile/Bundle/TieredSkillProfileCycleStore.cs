namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered skill profile cycles by code.</summary>
public interface ITieredSkillProfileCycleStore
{
    TieredSkillProfileCycle? Find(string code);
    void Save(TieredSkillProfileCycle item);
    bool Remove(string code);
    List<TieredSkillProfileCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSkillProfileCycleStore(Dictionary<string, TieredSkillProfileCycle> items, List<string> order) : ITieredSkillProfileCycleStore
{
    public Dictionary<string, TieredSkillProfileCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSkillProfileCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSkillProfileCycle item)
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

    public List<TieredSkillProfileCycle> List()
    {
        var outValue = new List<TieredSkillProfileCycle>();
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
public sealed class CachedTieredSkillProfileCycleStore(MemoryTieredSkillProfileCycleStore inner, TieredSkillProfileCycleCache cache, int hits, int misses) : ITieredSkillProfileCycleStore
{
    public MemoryTieredSkillProfileCycleStore Inner { get; set; } = inner;
    public TieredSkillProfileCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSkillProfileCycle? Find(string code)
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

    public void Save(TieredSkillProfileCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSkillProfileCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered skill profile cycles. Empty fields match all.</summary>
public sealed record TieredSkillProfileCycleQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSkillProfileCycle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredSkillProfileCycle> Apply(List<TieredSkillProfileCycle> items)
    {
        var matched = Collections.Filter(items, (TieredSkillProfileCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered skill profile cycle store.</summary>
public static class TieredSkillProfileCycleStore
{
    public static ITieredSkillProfileCycleStore NewMemoryTieredSkillProfileCycleStore()
    {
        return new MemoryTieredSkillProfileCycleStore(
            new Dictionary<string, TieredSkillProfileCycle>(),
            new List<string>());
    }

    public static ITieredSkillProfileCycleStore NewCachedTieredSkillProfileCycleStore(MemoryTieredSkillProfileCycleStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSkillProfileCycleStore(
            inner,
            TieredSkillProfileCycleCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<TieredSkillProfileCycle> FindTieredSkillProfileCycles(ITieredSkillProfileCycleStore store, TieredSkillProfileCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
