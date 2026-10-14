namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile tiers by code.</summary>
public interface ISkillProfileTierStore
{
    SkillProfileTier? Find(string code);
    void Save(SkillProfileTier item);
    bool Remove(string code);
    List<SkillProfileTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileTierStore(Dictionary<string, SkillProfileTier> items, List<string> order) : ISkillProfileTierStore
{
    public Dictionary<string, SkillProfileTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileTier item)
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

    public List<SkillProfileTier> List()
    {
        var outValue = new List<SkillProfileTier>();
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
public sealed class CachedSkillProfileTierStore(MemorySkillProfileTierStore inner, SkillProfileTierCache cache, int hits, int misses) : ISkillProfileTierStore
{
    public MemorySkillProfileTierStore Inner { get; set; } = inner;
    public SkillProfileTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileTier? Find(string code)
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

    public void Save(SkillProfileTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile tiers. Empty fields match all.</summary>
public sealed record SkillProfileTierQuery(string Category, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileTier item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileTier> Apply(List<SkillProfileTier> items)
    {
        var matched = Collections.Filter(items, (SkillProfileTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile tier store.</summary>
public static class SkillProfileTierStore
{
    public static ISkillProfileTierStore NewMemorySkillProfileTierStore()
    {
        return new MemorySkillProfileTierStore(new Dictionary<string, SkillProfileTier>(), new List<string>());
    }

    public static ISkillProfileTierStore NewCachedSkillProfileTierStore(MemorySkillProfileTierStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileTierStore(inner, SkillProfileTierCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SkillProfileTier> FindSkillProfileTiers(ISkillProfileTierStore store, SkillProfileTierQuery query)
    {
        return query.Apply(store.List());
    }
}
