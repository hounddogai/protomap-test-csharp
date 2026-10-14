namespace Acme.Workforce.Talent.SkillProfile.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile budgets by code.</summary>
public interface ISkillProfileBudgetStore
{
    SkillProfileBudget? Find(string code);
    void Save(SkillProfileBudget item);
    bool Remove(string code);
    List<SkillProfileBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileBudgetStore(Dictionary<string, SkillProfileBudget> items, List<string> order) : ISkillProfileBudgetStore
{
    public Dictionary<string, SkillProfileBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileBudget item)
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

    public List<SkillProfileBudget> List()
    {
        var outValue = new List<SkillProfileBudget>();
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
public sealed class CachedSkillProfileBudgetStore(MemorySkillProfileBudgetStore inner, SkillProfileBudgetCache cache, int hits, int misses) : ISkillProfileBudgetStore
{
    public MemorySkillProfileBudgetStore Inner { get; set; } = inner;
    public SkillProfileBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileBudget? Find(string code)
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

    public void Save(SkillProfileBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile budgets. Empty fields match all.</summary>
public sealed record SkillProfileBudgetQuery(string Title, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileBudget item)
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

    public List<SkillProfileBudget> Apply(List<SkillProfileBudget> items)
    {
        var matched = Collections.Filter(items, (SkillProfileBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile budget store.</summary>
public static class SkillProfileBudgetStore
{
    public static ISkillProfileBudgetStore NewMemorySkillProfileBudgetStore()
    {
        return new MemorySkillProfileBudgetStore(new Dictionary<string, SkillProfileBudget>(), new List<string>());
    }

    public static ISkillProfileBudgetStore NewCachedSkillProfileBudgetStore(MemorySkillProfileBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileBudgetStore(inner, SkillProfileBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<SkillProfileBudget> FindSkillProfileBudgets(ISkillProfileBudgetStore store, SkillProfileBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
