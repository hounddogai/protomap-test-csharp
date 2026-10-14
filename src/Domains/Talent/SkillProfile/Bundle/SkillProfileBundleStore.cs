namespace Acme.Workforce.Talent.SkillProfile.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile bundles by code.</summary>
public interface ISkillProfileBundleStore
{
    SkillProfileBundle? Find(string code);
    void Save(SkillProfileBundle item);
    bool Remove(string code);
    List<SkillProfileBundle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileBundleStore(Dictionary<string, SkillProfileBundle> items, List<string> order) : ISkillProfileBundleStore
{
    public Dictionary<string, SkillProfileBundle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileBundle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileBundle item)
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

    public List<SkillProfileBundle> List()
    {
        var outValue = new List<SkillProfileBundle>();
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
public sealed class CachedSkillProfileBundleStore(MemorySkillProfileBundleStore inner, SkillProfileBundleCache cache, int hits, int misses) : ISkillProfileBundleStore
{
    public MemorySkillProfileBundleStore Inner { get; set; } = inner;
    public SkillProfileBundleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileBundle? Find(string code)
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

    public void Save(SkillProfileBundle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileBundle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile bundles. Empty fields match all.</summary>
public sealed record SkillProfileBundleQuery(string Label, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileBundle item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileBundle> Apply(List<SkillProfileBundle> items)
    {
        var matched = Collections.Filter(items, (SkillProfileBundle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile bundle store.</summary>
public static class SkillProfileBundleStore
{
    public static ISkillProfileBundleStore NewMemorySkillProfileBundleStore()
    {
        return new MemorySkillProfileBundleStore(new Dictionary<string, SkillProfileBundle>(), new List<string>());
    }

    public static ISkillProfileBundleStore NewCachedSkillProfileBundleStore(MemorySkillProfileBundleStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileBundleStore(inner, SkillProfileBundleCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SkillProfileBundle> FindSkillProfileBundles(ISkillProfileBundleStore store, SkillProfileBundleQuery query)
    {
        return query.Apply(store.List());
    }
}
