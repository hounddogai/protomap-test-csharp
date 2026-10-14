namespace Acme.Workforce.Talent.SkillProfile.Target;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending skill profile targets by code.</summary>
public interface IPendingSkillProfileTargetStore
{
    PendingSkillProfileTarget? Find(string code);
    void Save(PendingSkillProfileTarget item);
    bool Remove(string code);
    List<PendingSkillProfileTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingSkillProfileTargetStore(Dictionary<string, PendingSkillProfileTarget> items, List<string> order) : IPendingSkillProfileTargetStore
{
    public Dictionary<string, PendingSkillProfileTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingSkillProfileTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingSkillProfileTarget item)
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

    public List<PendingSkillProfileTarget> List()
    {
        var outValue = new List<PendingSkillProfileTarget>();
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
public sealed class CachedPendingSkillProfileTargetStore(MemoryPendingSkillProfileTargetStore inner, PendingSkillProfileTargetCache cache, int hits, int misses) : IPendingSkillProfileTargetStore
{
    public MemoryPendingSkillProfileTargetStore Inner { get; set; } = inner;
    public PendingSkillProfileTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingSkillProfileTarget? Find(string code)
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

    public void Save(PendingSkillProfileTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingSkillProfileTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending skill profile targets. Empty fields match all.</summary>
public sealed record PendingSkillProfileTargetQuery(string Name, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PendingSkillProfileTarget item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingSkillProfileTarget> Apply(List<PendingSkillProfileTarget> items)
    {
        var matched = Collections.Filter(items, (PendingSkillProfileTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending skill profile target store.</summary>
public static class PendingSkillProfileTargetStore
{
    public static IPendingSkillProfileTargetStore NewMemoryPendingSkillProfileTargetStore()
    {
        return new MemoryPendingSkillProfileTargetStore(
            new Dictionary<string, PendingSkillProfileTarget>(),
            new List<string>());
    }

    public static IPendingSkillProfileTargetStore NewCachedPendingSkillProfileTargetStore(MemoryPendingSkillProfileTargetStore inner, int ttl, IClock clock)
    {
        return new CachedPendingSkillProfileTargetStore(
            inner,
            PendingSkillProfileTargetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PendingSkillProfileTarget> FindPendingSkillProfileTargets(IPendingSkillProfileTargetStore store, PendingSkillProfileTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
