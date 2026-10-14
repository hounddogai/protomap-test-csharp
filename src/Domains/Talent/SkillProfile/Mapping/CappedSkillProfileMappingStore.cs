namespace Acme.Workforce.Talent.SkillProfile.Mapping;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped skill profile mappings by code.</summary>
public interface ICappedSkillProfileMappingStore
{
    CappedSkillProfileMapping? Find(string code);
    void Save(CappedSkillProfileMapping item);
    bool Remove(string code);
    List<CappedSkillProfileMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedSkillProfileMappingStore(Dictionary<string, CappedSkillProfileMapping> items, List<string> order) : ICappedSkillProfileMappingStore
{
    public Dictionary<string, CappedSkillProfileMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedSkillProfileMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedSkillProfileMapping item)
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

    public List<CappedSkillProfileMapping> List()
    {
        var outValue = new List<CappedSkillProfileMapping>();
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
public sealed class CachedCappedSkillProfileMappingStore(MemoryCappedSkillProfileMappingStore inner, CappedSkillProfileMappingCache cache, int hits, int misses) : ICappedSkillProfileMappingStore
{
    public MemoryCappedSkillProfileMappingStore Inner { get; set; } = inner;
    public CappedSkillProfileMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedSkillProfileMapping? Find(string code)
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

    public void Save(CappedSkillProfileMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedSkillProfileMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped skill profile mappings. Empty fields match all.</summary>
public sealed record CappedSkillProfileMappingQuery(string Category, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedSkillProfileMapping item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedSkillProfileMapping> Apply(List<CappedSkillProfileMapping> items)
    {
        var matched = Collections.Filter(items, (CappedSkillProfileMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped skill profile mapping store.</summary>
public static class CappedSkillProfileMappingStore
{
    public static ICappedSkillProfileMappingStore NewMemoryCappedSkillProfileMappingStore()
    {
        return new MemoryCappedSkillProfileMappingStore(
            new Dictionary<string, CappedSkillProfileMapping>(),
            new List<string>());
    }

    public static ICappedSkillProfileMappingStore NewCachedCappedSkillProfileMappingStore(MemoryCappedSkillProfileMappingStore inner, int ttl, IClock clock)
    {
        return new CachedCappedSkillProfileMappingStore(
            inner,
            CappedSkillProfileMappingCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedSkillProfileMapping> FindCappedSkillProfileMappings(ICappedSkillProfileMappingStore store, CappedSkillProfileMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
