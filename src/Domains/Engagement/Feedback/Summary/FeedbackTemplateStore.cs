namespace Acme.Workforce.Engagement.Feedback.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps feedback templates by code.</summary>
public interface IFeedbackTemplateStore
{
    FeedbackTemplate? Find(string code);
    void Save(FeedbackTemplate item);
    bool Remove(string code);
    List<FeedbackTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryFeedbackTemplateStore(Dictionary<string, FeedbackTemplate> items, List<string> order) : IFeedbackTemplateStore
{
    public Dictionary<string, FeedbackTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public FeedbackTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(FeedbackTemplate item)
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

    public List<FeedbackTemplate> List()
    {
        var outValue = new List<FeedbackTemplate>();
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
public sealed class CachedFeedbackTemplateStore(MemoryFeedbackTemplateStore inner, FeedbackTemplateCache cache, int hits, int misses) : IFeedbackTemplateStore
{
    public MemoryFeedbackTemplateStore Inner { get; set; } = inner;
    public FeedbackTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public FeedbackTemplate? Find(string code)
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

    public void Save(FeedbackTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<FeedbackTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects feedback templates. Empty fields match all.</summary>
public sealed record FeedbackTemplateQuery(string Name, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(FeedbackTemplate item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<FeedbackTemplate> Apply(List<FeedbackTemplate> items)
    {
        var matched = Collections.Filter(items, (FeedbackTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of feedback template store.</summary>
public static class FeedbackTemplateStore
{
    public static IFeedbackTemplateStore NewMemoryFeedbackTemplateStore()
    {
        return new MemoryFeedbackTemplateStore(new Dictionary<string, FeedbackTemplate>(), new List<string>());
    }

    public static IFeedbackTemplateStore NewCachedFeedbackTemplateStore(MemoryFeedbackTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedFeedbackTemplateStore(inner, FeedbackTemplateCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<FeedbackTemplate> FindFeedbackTemplates(IFeedbackTemplateStore store, FeedbackTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
