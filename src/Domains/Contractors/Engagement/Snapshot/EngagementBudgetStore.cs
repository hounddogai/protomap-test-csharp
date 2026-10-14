namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps engagement budgets by code.</summary>
public interface IEngagementBudgetStore
{
    EngagementBudget? Find(string code);
    void Save(EngagementBudget item);
    bool Remove(string code);
    List<EngagementBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEngagementBudgetStore(Dictionary<string, EngagementBudget> items, List<string> order) : IEngagementBudgetStore
{
    public Dictionary<string, EngagementBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EngagementBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EngagementBudget item)
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

    public List<EngagementBudget> List()
    {
        var outValue = new List<EngagementBudget>();
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
public sealed class CachedEngagementBudgetStore(MemoryEngagementBudgetStore inner, EngagementBudgetCache cache, int hits, int misses) : IEngagementBudgetStore
{
    public MemoryEngagementBudgetStore Inner { get; set; } = inner;
    public EngagementBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EngagementBudget? Find(string code)
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

    public void Save(EngagementBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EngagementBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects engagement budgets. Empty fields match all.</summary>
public sealed record EngagementBudgetQuery(string Title, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(EngagementBudget item)
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

    public List<EngagementBudget> Apply(List<EngagementBudget> items)
    {
        var matched = Collections.Filter(items, (EngagementBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of engagement budget store.</summary>
public static class EngagementBudgetStore
{
    public static IEngagementBudgetStore NewMemoryEngagementBudgetStore()
    {
        return new MemoryEngagementBudgetStore(new Dictionary<string, EngagementBudget>(), new List<string>());
    }

    public static IEngagementBudgetStore NewCachedEngagementBudgetStore(MemoryEngagementBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedEngagementBudgetStore(inner, EngagementBudgetCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<EngagementBudget> FindEngagementBudgets(IEngagementBudgetStore store, EngagementBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
