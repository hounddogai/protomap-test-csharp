namespace Acme.Workforce.Talent.SkillProfile.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile rollups by code.</summary>
public interface ISkillProfileRollupStore
{
    SkillProfileRollup? Find(string code);
    void Save(SkillProfileRollup item);
    bool Remove(string code);
    List<SkillProfileRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileRollupStore(Dictionary<string, SkillProfileRollup> items, List<string> order) : ISkillProfileRollupStore
{
    public Dictionary<string, SkillProfileRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileRollup item)
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

    public List<SkillProfileRollup> List()
    {
        var outValue = new List<SkillProfileRollup>();
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
public sealed class CachedSkillProfileRollupStore(MemorySkillProfileRollupStore inner, SkillProfileRollupCache cache, int hits, int misses) : ISkillProfileRollupStore
{
    public MemorySkillProfileRollupStore Inner { get; set; } = inner;
    public SkillProfileRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileRollup? Find(string code)
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

    public void Save(SkillProfileRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile rollups. Empty fields match all.</summary>
public sealed record SkillProfileRollupQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileRollup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileRollup> Apply(List<SkillProfileRollup> items)
    {
        var matched = Collections.Filter(items, (SkillProfileRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile rollup store.</summary>
public static class SkillProfileRollupStore
{
    public static ISkillProfileRollupStore NewMemorySkillProfileRollupStore()
    {
        return new MemorySkillProfileRollupStore(new Dictionary<string, SkillProfileRollup>(), new List<string>());
    }

    public static ISkillProfileRollupStore NewCachedSkillProfileRollupStore(MemorySkillProfileRollupStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileRollupStore(inner, SkillProfileRollupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SkillProfileRollup> FindSkillProfileRollups(ISkillProfileRollupStore store, SkillProfileRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
