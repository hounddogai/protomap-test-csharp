namespace Acme.Workforce.Talent.SkillProfile.Forecast;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile manifests by code.</summary>
public interface ISkillProfileManifestStore
{
    SkillProfileManifest? Find(string code);
    void Save(SkillProfileManifest item);
    bool Remove(string code);
    List<SkillProfileManifest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileManifestStore(Dictionary<string, SkillProfileManifest> items, List<string> order) : ISkillProfileManifestStore
{
    public Dictionary<string, SkillProfileManifest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileManifest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileManifest item)
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

    public List<SkillProfileManifest> List()
    {
        var outValue = new List<SkillProfileManifest>();
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
public sealed class CachedSkillProfileManifestStore(MemorySkillProfileManifestStore inner, SkillProfileManifestCache cache, int hits, int misses) : ISkillProfileManifestStore
{
    public MemorySkillProfileManifestStore Inner { get; set; } = inner;
    public SkillProfileManifestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileManifest? Find(string code)
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

    public void Save(SkillProfileManifest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileManifest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile manifests. Empty fields match all.</summary>
public sealed record SkillProfileManifestQuery(string Team, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileManifest item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileManifest> Apply(List<SkillProfileManifest> items)
    {
        var matched = Collections.Filter(items, (SkillProfileManifest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile manifest store.</summary>
public static class SkillProfileManifestStore
{
    public static ISkillProfileManifestStore NewMemorySkillProfileManifestStore()
    {
        return new MemorySkillProfileManifestStore(
            new Dictionary<string, SkillProfileManifest>(),
            new List<string>());
    }

    public static ISkillProfileManifestStore NewCachedSkillProfileManifestStore(MemorySkillProfileManifestStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileManifestStore(inner, SkillProfileManifestCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SkillProfileManifest> FindSkillProfileManifests(ISkillProfileManifestStore store, SkillProfileManifestQuery query)
    {
        return query.Apply(store.List());
    }
}
