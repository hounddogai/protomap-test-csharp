namespace Acme.Workforce.Contractors.Engagement.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient engagement templates by code.</summary>
public interface ILenientEngagementTemplateStore
{
    LenientEngagementTemplate? Find(string code);
    void Save(LenientEngagementTemplate item);
    bool Remove(string code);
    List<LenientEngagementTemplate> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientEngagementTemplateStore(Dictionary<string, LenientEngagementTemplate> items, List<string> order) : ILenientEngagementTemplateStore
{
    public Dictionary<string, LenientEngagementTemplate> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientEngagementTemplate? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientEngagementTemplate item)
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

    public List<LenientEngagementTemplate> List()
    {
        var outValue = new List<LenientEngagementTemplate>();
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
public sealed class CachedLenientEngagementTemplateStore(MemoryLenientEngagementTemplateStore inner, LenientEngagementTemplateCache cache, int hits, int misses) : ILenientEngagementTemplateStore
{
    public MemoryLenientEngagementTemplateStore Inner { get; set; } = inner;
    public LenientEngagementTemplateCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientEngagementTemplate? Find(string code)
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

    public void Save(LenientEngagementTemplate item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientEngagementTemplate> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient engagement templates. Empty fields match all.</summary>
public sealed record LenientEngagementTemplateQuery(string Label, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(LenientEngagementTemplate item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LenientEngagementTemplate> Apply(List<LenientEngagementTemplate> items)
    {
        var matched = Collections.Filter(items, (LenientEngagementTemplate candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient engagement template store.</summary>
public static class LenientEngagementTemplateStore
{
    public static ILenientEngagementTemplateStore NewMemoryLenientEngagementTemplateStore()
    {
        return new MemoryLenientEngagementTemplateStore(
            new Dictionary<string, LenientEngagementTemplate>(),
            new List<string>());
    }

    public static ILenientEngagementTemplateStore NewCachedLenientEngagementTemplateStore(MemoryLenientEngagementTemplateStore inner, int ttl, IClock clock)
    {
        return new CachedLenientEngagementTemplateStore(
            inner,
            LenientEngagementTemplateCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientEngagementTemplate> FindLenientEngagementTemplates(ILenientEngagementTemplateStore store, LenientEngagementTemplateQuery query)
    {
        return query.Apply(store.List());
    }
}
