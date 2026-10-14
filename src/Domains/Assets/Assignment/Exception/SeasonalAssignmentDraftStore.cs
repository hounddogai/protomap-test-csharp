namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal assignment drafts by code.</summary>
public interface ISeasonalAssignmentDraftStore
{
    SeasonalAssignmentDraft? Find(string code);
    void Save(SeasonalAssignmentDraft item);
    bool Remove(string code);
    List<SeasonalAssignmentDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAssignmentDraftStore(Dictionary<string, SeasonalAssignmentDraft> items, List<string> order) : ISeasonalAssignmentDraftStore
{
    public Dictionary<string, SeasonalAssignmentDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAssignmentDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAssignmentDraft item)
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

    public List<SeasonalAssignmentDraft> List()
    {
        var outValue = new List<SeasonalAssignmentDraft>();
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
public sealed class CachedSeasonalAssignmentDraftStore(MemorySeasonalAssignmentDraftStore inner, SeasonalAssignmentDraftCache cache, int hits, int misses) : ISeasonalAssignmentDraftStore
{
    public MemorySeasonalAssignmentDraftStore Inner { get; set; } = inner;
    public SeasonalAssignmentDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAssignmentDraft? Find(string code)
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

    public void Save(SeasonalAssignmentDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAssignmentDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal assignment drafts. Empty fields match all.</summary>
public sealed record SeasonalAssignmentDraftQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAssignmentDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAssignmentDraft> Apply(List<SeasonalAssignmentDraft> items)
    {
        var matched = Collections.Filter(items, (SeasonalAssignmentDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal assignment draft store.</summary>
public static class SeasonalAssignmentDraftStore
{
    public static ISeasonalAssignmentDraftStore NewMemorySeasonalAssignmentDraftStore()
    {
        return new MemorySeasonalAssignmentDraftStore(
            new Dictionary<string, SeasonalAssignmentDraft>(),
            new List<string>());
    }

    public static ISeasonalAssignmentDraftStore NewCachedSeasonalAssignmentDraftStore(MemorySeasonalAssignmentDraftStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAssignmentDraftStore(
            inner,
            SeasonalAssignmentDraftCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAssignmentDraft> FindSeasonalAssignmentDrafts(ISeasonalAssignmentDraftStore store, SeasonalAssignmentDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
