namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile thresholds by code.</summary>
public interface ISkillProfileThresholdStore
{
    SkillProfileThreshold? Find(string code);
    void Save(SkillProfileThreshold item);
    bool Remove(string code);
    List<SkillProfileThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileThresholdStore(Dictionary<string, SkillProfileThreshold> items, List<string> order) : ISkillProfileThresholdStore
{
    public Dictionary<string, SkillProfileThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileThreshold item)
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

    public List<SkillProfileThreshold> List()
    {
        var outValue = new List<SkillProfileThreshold>();
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
public sealed class CachedSkillProfileThresholdStore(MemorySkillProfileThresholdStore inner, SkillProfileThresholdCache cache, int hits, int misses) : ISkillProfileThresholdStore
{
    public MemorySkillProfileThresholdStore Inner { get; set; } = inner;
    public SkillProfileThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileThreshold? Find(string code)
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

    public void Save(SkillProfileThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile thresholds. Empty fields match all.</summary>
public sealed record SkillProfileThresholdQuery(string Label, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileThreshold item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileThreshold> Apply(List<SkillProfileThreshold> items)
    {
        var matched = Collections.Filter(items, (SkillProfileThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile threshold store.</summary>
public static class SkillProfileThresholdStore
{
    public static ISkillProfileThresholdStore NewMemorySkillProfileThresholdStore()
    {
        return new MemorySkillProfileThresholdStore(
            new Dictionary<string, SkillProfileThreshold>(),
            new List<string>());
    }

    public static ISkillProfileThresholdStore NewCachedSkillProfileThresholdStore(MemorySkillProfileThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileThresholdStore(inner, SkillProfileThresholdCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SkillProfileThreshold> FindSkillProfileThresholds(ISkillProfileThresholdStore store, SkillProfileThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
