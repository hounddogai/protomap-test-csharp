namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile targets by code.</summary>
public interface ISkillProfileTargetStore
{
    SkillProfileTarget? Find(string code);
    void Save(SkillProfileTarget item);
    bool Remove(string code);
    List<SkillProfileTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileTargetStore(Dictionary<string, SkillProfileTarget> items, List<string> order) : ISkillProfileTargetStore
{
    public Dictionary<string, SkillProfileTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileTarget item)
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

    public List<SkillProfileTarget> List()
    {
        var outValue = new List<SkillProfileTarget>();
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
public sealed class CachedSkillProfileTargetStore(MemorySkillProfileTargetStore inner, SkillProfileTargetCache cache, int hits, int misses) : ISkillProfileTargetStore
{
    public MemorySkillProfileTargetStore Inner { get; set; } = inner;
    public SkillProfileTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileTarget? Find(string code)
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

    public void Save(SkillProfileTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile targets. Empty fields match all.</summary>
public sealed record SkillProfileTargetQuery(string Unit, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileTarget item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileTarget> Apply(List<SkillProfileTarget> items)
    {
        var matched = Collections.Filter(items, (SkillProfileTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile target store.</summary>
public static class SkillProfileTargetStore
{
    public static ISkillProfileTargetStore NewMemorySkillProfileTargetStore()
    {
        return new MemorySkillProfileTargetStore(new Dictionary<string, SkillProfileTarget>(), new List<string>());
    }

    public static ISkillProfileTargetStore NewCachedSkillProfileTargetStore(MemorySkillProfileTargetStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileTargetStore(inner, SkillProfileTargetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SkillProfileTarget> FindSkillProfileTargets(ISkillProfileTargetStore store, SkillProfileTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
