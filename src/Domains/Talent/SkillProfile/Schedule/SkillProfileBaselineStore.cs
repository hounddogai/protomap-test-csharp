namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile baselines by code.</summary>
public interface ISkillProfileBaselineStore
{
    SkillProfileBaseline? Find(string code);
    void Save(SkillProfileBaseline item);
    bool Remove(string code);
    List<SkillProfileBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileBaselineStore(Dictionary<string, SkillProfileBaseline> items, List<string> order) : ISkillProfileBaselineStore
{
    public Dictionary<string, SkillProfileBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileBaseline item)
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

    public List<SkillProfileBaseline> List()
    {
        var outValue = new List<SkillProfileBaseline>();
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
public sealed class CachedSkillProfileBaselineStore(MemorySkillProfileBaselineStore inner, SkillProfileBaselineCache cache, int hits, int misses) : ISkillProfileBaselineStore
{
    public MemorySkillProfileBaselineStore Inner { get; set; } = inner;
    public SkillProfileBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileBaseline? Find(string code)
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

    public void Save(SkillProfileBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile baselines. Empty fields match all.</summary>
public sealed record SkillProfileBaselineQuery(string Unit, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileBaseline item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileBaseline> Apply(List<SkillProfileBaseline> items)
    {
        var matched = Collections.Filter(items, (SkillProfileBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile baseline store.</summary>
public static class SkillProfileBaselineStore
{
    public static ISkillProfileBaselineStore NewMemorySkillProfileBaselineStore()
    {
        return new MemorySkillProfileBaselineStore(
            new Dictionary<string, SkillProfileBaseline>(),
            new List<string>());
    }

    public static ISkillProfileBaselineStore NewCachedSkillProfileBaselineStore(MemorySkillProfileBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileBaselineStore(inner, SkillProfileBaselineCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<SkillProfileBaseline> FindSkillProfileBaselines(ISkillProfileBaselineStore store, SkillProfileBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
