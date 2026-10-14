namespace Acme.Workforce.Talent.SkillProfile.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile groups by code.</summary>
public interface ISkillProfileGroupStore
{
    SkillProfileGroup? Find(string code);
    void Save(SkillProfileGroup item);
    bool Remove(string code);
    List<SkillProfileGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileGroupStore(Dictionary<string, SkillProfileGroup> items, List<string> order) : ISkillProfileGroupStore
{
    public Dictionary<string, SkillProfileGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileGroup item)
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

    public List<SkillProfileGroup> List()
    {
        var outValue = new List<SkillProfileGroup>();
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
public sealed class CachedSkillProfileGroupStore(MemorySkillProfileGroupStore inner, SkillProfileGroupCache cache, int hits, int misses) : ISkillProfileGroupStore
{
    public MemorySkillProfileGroupStore Inner { get; set; } = inner;
    public SkillProfileGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileGroup? Find(string code)
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

    public void Save(SkillProfileGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile groups. Empty fields match all.</summary>
public sealed record SkillProfileGroupQuery(string Name, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<SkillProfileGroup> Apply(List<SkillProfileGroup> items)
    {
        var matched = Collections.Filter(items, (SkillProfileGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile group store.</summary>
public static class SkillProfileGroupStore
{
    public static ISkillProfileGroupStore NewMemorySkillProfileGroupStore()
    {
        return new MemorySkillProfileGroupStore(new Dictionary<string, SkillProfileGroup>(), new List<string>());
    }

    public static ISkillProfileGroupStore NewCachedSkillProfileGroupStore(MemorySkillProfileGroupStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileGroupStore(inner, SkillProfileGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SkillProfileGroup> FindSkillProfileGroups(ISkillProfileGroupStore store, SkillProfileGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
