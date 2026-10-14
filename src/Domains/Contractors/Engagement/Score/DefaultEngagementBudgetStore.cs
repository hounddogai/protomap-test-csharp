namespace Acme.Workforce.Contractors.Engagement.Score;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default engagement budgets by code.</summary>
public interface IDefaultEngagementBudgetStore
{
    DefaultEngagementBudget? Find(string code);
    void Save(DefaultEngagementBudget item);
    bool Remove(string code);
    List<DefaultEngagementBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultEngagementBudgetStore(Dictionary<string, DefaultEngagementBudget> items, List<string> order) : IDefaultEngagementBudgetStore
{
    public Dictionary<string, DefaultEngagementBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultEngagementBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultEngagementBudget item)
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

    public List<DefaultEngagementBudget> List()
    {
        var outValue = new List<DefaultEngagementBudget>();
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
public sealed class CachedDefaultEngagementBudgetStore(MemoryDefaultEngagementBudgetStore inner, DefaultEngagementBudgetCache cache, int hits, int misses) : IDefaultEngagementBudgetStore
{
    public MemoryDefaultEngagementBudgetStore Inner { get; set; } = inner;
    public DefaultEngagementBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultEngagementBudget? Find(string code)
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

    public void Save(DefaultEngagementBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultEngagementBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default engagement budgets. Empty fields match all.</summary>
public sealed record DefaultEngagementBudgetQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultEngagementBudget item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultEngagementBudget> Apply(List<DefaultEngagementBudget> items)
    {
        var matched = Collections.Filter(items, (DefaultEngagementBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default engagement budget store.</summary>
public static class DefaultEngagementBudgetStore
{
    public static IDefaultEngagementBudgetStore NewMemoryDefaultEngagementBudgetStore()
    {
        return new MemoryDefaultEngagementBudgetStore(
            new Dictionary<string, DefaultEngagementBudget>(),
            new List<string>());
    }

    public static IDefaultEngagementBudgetStore NewCachedDefaultEngagementBudgetStore(MemoryDefaultEngagementBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultEngagementBudgetStore(
            inner,
            DefaultEngagementBudgetCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<DefaultEngagementBudget> FindDefaultEngagementBudgets(IDefaultEngagementBudgetStore store, DefaultEngagementBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
