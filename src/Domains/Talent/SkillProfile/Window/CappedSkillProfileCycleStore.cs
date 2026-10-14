namespace Acme.Workforce.Talent.SkillProfile.Window;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped skill profile cycles by code.</summary>
public interface ICappedSkillProfileCycleStore
{
    CappedSkillProfileCycle? Find(string code);
    void Save(CappedSkillProfileCycle item);
    bool Remove(string code);
    List<CappedSkillProfileCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSkillProfileCycleStore(Dictionary<string, CappedSkillProfileCycle> items, List<string> order) : ICappedSkillProfileCycleStore
{
    public Dictionary<string, CappedSkillProfileCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSkillProfileCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSkillProfileCycle item)
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

    public List<CappedSkillProfileCycle> List()
    {
        var outValue = new List<CappedSkillProfileCycle>();
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
public sealed class CachedCappedSkillProfileCycleStore(MemoryCappedSkillProfileCycleStore inner, CappedSkillProfileCycleCache cache, int hits, int misses) : ICappedSkillProfileCycleStore
{
    public MemoryCappedSkillProfileCycleStore Inner { get; set; } = inner;
    public CappedSkillProfileCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSkillProfileCycle? Find(string code)
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

    public void Save(CappedSkillProfileCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSkillProfileCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped skill profile cycles. Empty fields match all.</summary>
public sealed record CappedSkillProfileCycleQuery(string Unit, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSkillProfileCycle item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSkillProfileCycle> Apply(List<CappedSkillProfileCycle> items)
    {
        var matched = Collections.Filter(items, (CappedSkillProfileCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped skill profile cycle store.</summary>
public static class CappedSkillProfileCycleStore
{
    public static ICappedSkillProfileCycleStore NewMemoryCappedSkillProfileCycleStore()
    {
        return new MemoryCappedSkillProfileCycleStore(
            new Dictionary<string, CappedSkillProfileCycle>(),
            new List<string>());
    }

    public static ICappedSkillProfileCycleStore NewCachedCappedSkillProfileCycleStore(MemoryCappedSkillProfileCycleStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSkillProfileCycleStore(
            inner,
            CappedSkillProfileCycleCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<CappedSkillProfileCycle> FindCappedSkillProfileCycles(ICappedSkillProfileCycleStore store, CappedSkillProfileCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
