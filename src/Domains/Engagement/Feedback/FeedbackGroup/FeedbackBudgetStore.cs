namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback budgets by code.</summary>
public interface IFeedbackBudgetStore
{
    FeedbackBudget? Find(string code);
    void Save(FeedbackBudget item);
    bool Remove(string code);
    List<FeedbackBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackBudgetStore(Dictionary<string, FeedbackBudget> items, List<string> order) : IFeedbackBudgetStore
{
    public Dictionary<string, FeedbackBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackBudget item)
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

    public List<FeedbackBudget> List()
    {
        var outValue = new List<FeedbackBudget>();
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
public sealed class CachedFeedbackBudgetStore(MemoryFeedbackBudgetStore inner, FeedbackBudgetCache cache, int hits, int misses) : IFeedbackBudgetStore
{
    public MemoryFeedbackBudgetStore Inner { get; set; } = inner;
    public FeedbackBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackBudget? Find(string code)
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

    public void Save(FeedbackBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback budgets. Empty fields match all.</summary>
public sealed record FeedbackBudgetQuery(string Segment, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackBudget item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<FeedbackBudget> Apply(List<FeedbackBudget> items)
    {
        var matched = Collections.Filter(items, (FeedbackBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback budget store.</summary>
public static class FeedbackBudgetStore
{
    public static IFeedbackBudgetStore NewMemoryFeedbackBudgetStore()
    {
        return new MemoryFeedbackBudgetStore(new Dictionary<string, FeedbackBudget>(), new List<string>());
    }

    public static IFeedbackBudgetStore NewCachedFeedbackBudgetStore(MemoryFeedbackBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackBudgetStore(inner, FeedbackBudgetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<FeedbackBudget> FindFeedbackBudgets(IFeedbackBudgetStore store, FeedbackBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
