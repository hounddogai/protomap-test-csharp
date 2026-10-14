namespace Acme.Workforce.Talent.SkillProfile.Adjustment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted skill profile budgets by code.</summary>
public interface IWeightedSkillProfileBudgetStore
{
    WeightedSkillProfileBudget? Find(string code);
    void Save(WeightedSkillProfileBudget item);
    bool Remove(string code);
    List<WeightedSkillProfileBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedSkillProfileBudgetStore(Dictionary<string, WeightedSkillProfileBudget> items, List<string> order) : IWeightedSkillProfileBudgetStore
{
    public Dictionary<string, WeightedSkillProfileBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedSkillProfileBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedSkillProfileBudget item)
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

    public List<WeightedSkillProfileBudget> List()
    {
        var outValue = new List<WeightedSkillProfileBudget>();
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
public sealed class CachedWeightedSkillProfileBudgetStore(MemoryWeightedSkillProfileBudgetStore inner, WeightedSkillProfileBudgetCache cache, int hits, int misses) : IWeightedSkillProfileBudgetStore
{
    public MemoryWeightedSkillProfileBudgetStore Inner { get; set; } = inner;
    public WeightedSkillProfileBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedSkillProfileBudget? Find(string code)
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

    public void Save(WeightedSkillProfileBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedSkillProfileBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted skill profile budgets. Empty fields match all.</summary>
public sealed record WeightedSkillProfileBudgetQuery(string Title, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedSkillProfileBudget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<WeightedSkillProfileBudget> Apply(List<WeightedSkillProfileBudget> items)
    {
        var matched = Collections.Filter(items, (WeightedSkillProfileBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted skill profile budget store.</summary>
public static class WeightedSkillProfileBudgetStore
{
    public static IWeightedSkillProfileBudgetStore NewMemoryWeightedSkillProfileBudgetStore()
    {
        return new MemoryWeightedSkillProfileBudgetStore(
            new Dictionary<string, WeightedSkillProfileBudget>(),
            new List<string>());
    }

    public static IWeightedSkillProfileBudgetStore NewCachedWeightedSkillProfileBudgetStore(MemoryWeightedSkillProfileBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedSkillProfileBudgetStore(
            inner,
            WeightedSkillProfileBudgetCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<WeightedSkillProfileBudget> FindWeightedSkillProfileBudgets(IWeightedSkillProfileBudgetStore store, WeightedSkillProfileBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
