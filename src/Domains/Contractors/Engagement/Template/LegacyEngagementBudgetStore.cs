namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy engagement budgets by code.</summary>
public interface ILegacyEngagementBudgetStore
{
    LegacyEngagementBudget? Find(string code);
    void Save(LegacyEngagementBudget item);
    bool Remove(string code);
    List<LegacyEngagementBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyEngagementBudgetStore(Dictionary<string, LegacyEngagementBudget> items, List<string> order) : ILegacyEngagementBudgetStore
{
    public Dictionary<string, LegacyEngagementBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyEngagementBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyEngagementBudget item)
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

    public List<LegacyEngagementBudget> List()
    {
        var outValue = new List<LegacyEngagementBudget>();
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
public sealed class CachedLegacyEngagementBudgetStore(MemoryLegacyEngagementBudgetStore inner, LegacyEngagementBudgetCache cache, int hits, int misses) : ILegacyEngagementBudgetStore
{
    public MemoryLegacyEngagementBudgetStore Inner { get; set; } = inner;
    public LegacyEngagementBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyEngagementBudget? Find(string code)
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

    public void Save(LegacyEngagementBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyEngagementBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy engagement budgets. Empty fields match all.</summary>
public sealed record LegacyEngagementBudgetQuery(string Country, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyEngagementBudget item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyEngagementBudget> Apply(List<LegacyEngagementBudget> items)
    {
        var matched = Collections.Filter(items, (LegacyEngagementBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy engagement budget store.</summary>
public static class LegacyEngagementBudgetStore
{
    public static ILegacyEngagementBudgetStore NewMemoryLegacyEngagementBudgetStore()
    {
        return new MemoryLegacyEngagementBudgetStore(
            new Dictionary<string, LegacyEngagementBudget>(),
            new List<string>());
    }

    public static ILegacyEngagementBudgetStore NewCachedLegacyEngagementBudgetStore(MemoryLegacyEngagementBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyEngagementBudgetStore(
            inner,
            LegacyEngagementBudgetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyEngagementBudget> FindLegacyEngagementBudgets(ILegacyEngagementBudgetStore store, LegacyEngagementBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
