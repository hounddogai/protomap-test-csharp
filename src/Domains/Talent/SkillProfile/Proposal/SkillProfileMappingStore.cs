namespace Acme.Workforce.Talent.SkillProfile.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile mappings by code.</summary>
public interface ISkillProfileMappingStore
{
    SkillProfileMapping? Find(string code);
    void Save(SkillProfileMapping item);
    bool Remove(string code);
    List<SkillProfileMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileMappingStore(Dictionary<string, SkillProfileMapping> items, List<string> order) : ISkillProfileMappingStore
{
    public Dictionary<string, SkillProfileMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileMapping item)
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

    public List<SkillProfileMapping> List()
    {
        var outValue = new List<SkillProfileMapping>();
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
public sealed class CachedSkillProfileMappingStore(MemorySkillProfileMappingStore inner, SkillProfileMappingCache cache, int hits, int misses) : ISkillProfileMappingStore
{
    public MemorySkillProfileMappingStore Inner { get; set; } = inner;
    public SkillProfileMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileMapping? Find(string code)
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

    public void Save(SkillProfileMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile mappings. Empty fields match all.</summary>
public sealed record SkillProfileMappingQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileMapping item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileMapping> Apply(List<SkillProfileMapping> items)
    {
        var matched = Collections.Filter(items, (SkillProfileMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile mapping store.</summary>
public static class SkillProfileMappingStore
{
    public static ISkillProfileMappingStore NewMemorySkillProfileMappingStore()
    {
        return new MemorySkillProfileMappingStore(new Dictionary<string, SkillProfileMapping>(), new List<string>());
    }

    public static ISkillProfileMappingStore NewCachedSkillProfileMappingStore(MemorySkillProfileMappingStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileMappingStore(inner, SkillProfileMappingCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SkillProfileMapping> FindSkillProfileMappings(ISkillProfileMappingStore store, SkillProfileMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
