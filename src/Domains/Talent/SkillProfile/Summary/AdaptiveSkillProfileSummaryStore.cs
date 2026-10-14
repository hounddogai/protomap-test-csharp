namespace Acme.Workforce.Talent.SkillProfile.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adaptive skill profile summaries by code.</summary>
public interface IAdaptiveSkillProfileSummaryStore
{
    AdaptiveSkillProfileSummary? Find(string code);
    void Save(AdaptiveSkillProfileSummary item);
    bool Remove(string code);
    List<AdaptiveSkillProfileSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdaptiveSkillProfileSummaryStore(Dictionary<string, AdaptiveSkillProfileSummary> items, List<string> order) : IAdaptiveSkillProfileSummaryStore
{
    public Dictionary<string, AdaptiveSkillProfileSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdaptiveSkillProfileSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdaptiveSkillProfileSummary item)
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

    public List<AdaptiveSkillProfileSummary> List()
    {
        var outValue = new List<AdaptiveSkillProfileSummary>();
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
public sealed class CachedAdaptiveSkillProfileSummaryStore(MemoryAdaptiveSkillProfileSummaryStore inner, AdaptiveSkillProfileSummaryCache cache, int hits, int misses) : IAdaptiveSkillProfileSummaryStore
{
    public MemoryAdaptiveSkillProfileSummaryStore Inner { get; set; } = inner;
    public AdaptiveSkillProfileSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdaptiveSkillProfileSummary? Find(string code)
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

    public void Save(AdaptiveSkillProfileSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdaptiveSkillProfileSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adaptive skill profile summaries. Empty fields match all.</summary>
public sealed record AdaptiveSkillProfileSummaryQuery(string Name, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AdaptiveSkillProfileSummary item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdaptiveSkillProfileSummary> Apply(List<AdaptiveSkillProfileSummary> items)
    {
        var matched = Collections.Filter(items, (AdaptiveSkillProfileSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adaptive skill profile summary store.</summary>
public static class AdaptiveSkillProfileSummaryStore
{
    public static IAdaptiveSkillProfileSummaryStore NewMemoryAdaptiveSkillProfileSummaryStore()
    {
        return new MemoryAdaptiveSkillProfileSummaryStore(
            new Dictionary<string, AdaptiveSkillProfileSummary>(),
            new List<string>());
    }

    public static IAdaptiveSkillProfileSummaryStore NewCachedAdaptiveSkillProfileSummaryStore(MemoryAdaptiveSkillProfileSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedAdaptiveSkillProfileSummaryStore(
            inner,
            AdaptiveSkillProfileSummaryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<AdaptiveSkillProfileSummary> FindAdaptiveSkillProfileSummaries(IAdaptiveSkillProfileSummaryStore store, AdaptiveSkillProfileSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
