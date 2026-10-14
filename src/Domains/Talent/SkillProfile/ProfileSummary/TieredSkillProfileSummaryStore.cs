namespace Acme.Workforce.Talent.SkillProfile.ProfileSummary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered skill profile summaries by code.</summary>
public interface ITieredSkillProfileSummaryStore
{
    TieredSkillProfileSummary? Find(string code);
    void Save(TieredSkillProfileSummary item);
    bool Remove(string code);
    List<TieredSkillProfileSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredSkillProfileSummaryStore(Dictionary<string, TieredSkillProfileSummary> items, List<string> order) : ITieredSkillProfileSummaryStore
{
    public Dictionary<string, TieredSkillProfileSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredSkillProfileSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredSkillProfileSummary item)
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

    public List<TieredSkillProfileSummary> List()
    {
        var outValue = new List<TieredSkillProfileSummary>();
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
public sealed class CachedTieredSkillProfileSummaryStore(MemoryTieredSkillProfileSummaryStore inner, TieredSkillProfileSummaryCache cache, int hits, int misses) : ITieredSkillProfileSummaryStore
{
    public MemoryTieredSkillProfileSummaryStore Inner { get; set; } = inner;
    public TieredSkillProfileSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredSkillProfileSummary? Find(string code)
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

    public void Save(TieredSkillProfileSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredSkillProfileSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered skill profile summaries. Empty fields match all.</summary>
public sealed record TieredSkillProfileSummaryQuery(string Team, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(TieredSkillProfileSummary item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredSkillProfileSummary> Apply(List<TieredSkillProfileSummary> items)
    {
        var matched = Collections.Filter(items, (TieredSkillProfileSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered skill profile summary store.</summary>
public static class TieredSkillProfileSummaryStore
{
    public static ITieredSkillProfileSummaryStore NewMemoryTieredSkillProfileSummaryStore()
    {
        return new MemoryTieredSkillProfileSummaryStore(
            new Dictionary<string, TieredSkillProfileSummary>(),
            new List<string>());
    }

    public static ITieredSkillProfileSummaryStore NewCachedTieredSkillProfileSummaryStore(MemoryTieredSkillProfileSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedTieredSkillProfileSummaryStore(
            inner,
            TieredSkillProfileSummaryCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<TieredSkillProfileSummary> FindTieredSkillProfileSummaries(ITieredSkillProfileSummaryStore store, TieredSkillProfileSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
