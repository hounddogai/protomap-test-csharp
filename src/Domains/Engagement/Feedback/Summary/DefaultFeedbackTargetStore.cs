namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default feedback targets by code.</summary>
public interface IDefaultFeedbackTargetStore
{
    DefaultFeedbackTarget? Find(string code);
    void Save(DefaultFeedbackTarget item);
    bool Remove(string code);
    List<DefaultFeedbackTarget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultFeedbackTargetStore(Dictionary<string, DefaultFeedbackTarget> items, List<string> order) : IDefaultFeedbackTargetStore
{
    public Dictionary<string, DefaultFeedbackTarget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultFeedbackTarget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultFeedbackTarget item)
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

    public List<DefaultFeedbackTarget> List()
    {
        var outValue = new List<DefaultFeedbackTarget>();
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
public sealed class CachedDefaultFeedbackTargetStore(MemoryDefaultFeedbackTargetStore inner, DefaultFeedbackTargetCache cache, int hits, int misses) : IDefaultFeedbackTargetStore
{
    public MemoryDefaultFeedbackTargetStore Inner { get; set; } = inner;
    public DefaultFeedbackTargetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultFeedbackTarget? Find(string code)
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

    public void Save(DefaultFeedbackTarget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultFeedbackTarget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default feedback targets. Empty fields match all.</summary>
public sealed record DefaultFeedbackTargetQuery(string Title, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultFeedbackTarget item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultFeedbackTarget> Apply(List<DefaultFeedbackTarget> items)
    {
        var matched = Collections.Filter(items, (DefaultFeedbackTarget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default feedback target store.</summary>
public static class DefaultFeedbackTargetStore
{
    public static IDefaultFeedbackTargetStore NewMemoryDefaultFeedbackTargetStore()
    {
        return new MemoryDefaultFeedbackTargetStore(
            new Dictionary<string, DefaultFeedbackTarget>(),
            new List<string>());
    }

    public static IDefaultFeedbackTargetStore NewCachedDefaultFeedbackTargetStore(MemoryDefaultFeedbackTargetStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultFeedbackTargetStore(inner, DefaultFeedbackTargetCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<DefaultFeedbackTarget> FindDefaultFeedbackTargets(IDefaultFeedbackTargetStore store, DefaultFeedbackTargetQuery query)
    {
        return query.Apply(store.List());
    }
}
