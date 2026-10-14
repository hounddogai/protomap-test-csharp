namespace Acme.Workforce.Talent.CareerPath.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path rules by code.</summary>
public interface ICareerPathRuleStore
{
    CareerPathRule? Find(string code);
    void Save(CareerPathRule item);
    bool Remove(string code);
    List<CareerPathRule> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathRuleStore(Dictionary<string, CareerPathRule> items, List<string> order) : ICareerPathRuleStore
{
    public Dictionary<string, CareerPathRule> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathRule? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathRule item)
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

    public List<CareerPathRule> List()
    {
        var outValue = new List<CareerPathRule>();
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
public sealed class CachedCareerPathRuleStore(MemoryCareerPathRuleStore inner, CareerPathRuleCache cache, int hits, int misses) : ICareerPathRuleStore
{
    public MemoryCareerPathRuleStore Inner { get; set; } = inner;
    public CareerPathRuleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathRule? Find(string code)
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

    public void Save(CareerPathRule item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathRule> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path rules. Empty fields match all.</summary>
public sealed record CareerPathRuleQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathRule item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CareerPathRule> Apply(List<CareerPathRule> items)
    {
        var matched = Collections.Filter(items, (CareerPathRule candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path rule store.</summary>
public static class CareerPathRuleStore
{
    public static ICareerPathRuleStore NewMemoryCareerPathRuleStore()
    {
        return new MemoryCareerPathRuleStore(new Dictionary<string, CareerPathRule>(), new List<string>());
    }

    public static ICareerPathRuleStore NewCachedCareerPathRuleStore(MemoryCareerPathRuleStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathRuleStore(inner, CareerPathRuleCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<CareerPathRule> FindCareerPathRules(ICareerPathRuleStore store, CareerPathRuleQuery query)
    {
        return query.Apply(store.List());
    }
}
