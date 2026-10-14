namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped feedback groups by code.</summary>
public interface ICappedFeedbackGroupStore
{
    CappedFeedbackGroup? Find(string code);
    void Save(CappedFeedbackGroup item);
    bool Remove(string code);
    List<CappedFeedbackGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedFeedbackGroupStore(Dictionary<string, CappedFeedbackGroup> items, List<string> order) : ICappedFeedbackGroupStore
{
    public Dictionary<string, CappedFeedbackGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedFeedbackGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedFeedbackGroup item)
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

    public List<CappedFeedbackGroup> List()
    {
        var outValue = new List<CappedFeedbackGroup>();
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
public sealed class CachedCappedFeedbackGroupStore(MemoryCappedFeedbackGroupStore inner, CappedFeedbackGroupCache cache, int hits, int misses) : ICappedFeedbackGroupStore
{
    public MemoryCappedFeedbackGroupStore Inner { get; set; } = inner;
    public CappedFeedbackGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedFeedbackGroup? Find(string code)
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

    public void Save(CappedFeedbackGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedFeedbackGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped feedback groups. Empty fields match all.</summary>
public sealed record CappedFeedbackGroupQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(CappedFeedbackGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CappedFeedbackGroup> Apply(List<CappedFeedbackGroup> items)
    {
        var matched = Collections.Filter(items, (CappedFeedbackGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped feedback group store.</summary>
public static class CappedFeedbackGroupStore
{
    public static ICappedFeedbackGroupStore NewMemoryCappedFeedbackGroupStore()
    {
        return new MemoryCappedFeedbackGroupStore(new Dictionary<string, CappedFeedbackGroup>(), new List<string>());
    }

    public static ICappedFeedbackGroupStore NewCachedCappedFeedbackGroupStore(MemoryCappedFeedbackGroupStore inner, int ttl, IClock clock)
    {
        return new CachedCappedFeedbackGroupStore(inner, CappedFeedbackGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<CappedFeedbackGroup> FindCappedFeedbackGroups(ICappedFeedbackGroupStore store, CappedFeedbackGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
