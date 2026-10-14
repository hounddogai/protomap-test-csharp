namespace Acme.Workforce.Talent.SkillProfile.ProfileProposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps skill profile summaries by code.</summary>
public interface ISkillProfileSummaryStore
{
    SkillProfileSummary? Find(string code);
    void Save(SkillProfileSummary item);
    bool Remove(string code);
    List<SkillProfileSummary> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySkillProfileSummaryStore(Dictionary<string, SkillProfileSummary> items, List<string> order) : ISkillProfileSummaryStore
{
    public Dictionary<string, SkillProfileSummary> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SkillProfileSummary? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SkillProfileSummary item)
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

    public List<SkillProfileSummary> List()
    {
        var outValue = new List<SkillProfileSummary>();
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
public sealed class CachedSkillProfileSummaryStore(MemorySkillProfileSummaryStore inner, SkillProfileSummaryCache cache, int hits, int misses) : ISkillProfileSummaryStore
{
    public MemorySkillProfileSummaryStore Inner { get; set; } = inner;
    public SkillProfileSummaryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SkillProfileSummary? Find(string code)
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

    public void Save(SkillProfileSummary item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SkillProfileSummary> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects skill profile summaries. Empty fields match all.</summary>
public sealed record SkillProfileSummaryQuery(string Team, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(SkillProfileSummary item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SkillProfileSummary> Apply(List<SkillProfileSummary> items)
    {
        var matched = Collections.Filter(items, (SkillProfileSummary candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of skill profile summary store.</summary>
public static class SkillProfileSummaryStore
{
    public static ISkillProfileSummaryStore NewMemorySkillProfileSummaryStore()
    {
        return new MemorySkillProfileSummaryStore(new Dictionary<string, SkillProfileSummary>(), new List<string>());
    }

    public static ISkillProfileSummaryStore NewCachedSkillProfileSummaryStore(MemorySkillProfileSummaryStore inner, int ttl, IClock clock)
    {
        return new CachedSkillProfileSummaryStore(inner, SkillProfileSummaryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SkillProfileSummary> FindSkillProfileSummaries(ISkillProfileSummaryStore store, SkillProfileSummaryQuery query)
    {
        return query.Apply(store.List());
    }
}
