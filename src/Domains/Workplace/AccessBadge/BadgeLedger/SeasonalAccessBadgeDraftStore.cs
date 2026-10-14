namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal access badge drafts by code.</summary>
public interface ISeasonalAccessBadgeDraftStore
{
    SeasonalAccessBadgeDraft? Find(string code);
    void Save(SeasonalAccessBadgeDraft item);
    bool Remove(string code);
    List<SeasonalAccessBadgeDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAccessBadgeDraftStore(Dictionary<string, SeasonalAccessBadgeDraft> items, List<string> order) : ISeasonalAccessBadgeDraftStore
{
    public Dictionary<string, SeasonalAccessBadgeDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAccessBadgeDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAccessBadgeDraft item)
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

    public List<SeasonalAccessBadgeDraft> List()
    {
        var outValue = new List<SeasonalAccessBadgeDraft>();
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
public sealed class CachedSeasonalAccessBadgeDraftStore(MemorySeasonalAccessBadgeDraftStore inner, SeasonalAccessBadgeDraftCache cache, int hits, int misses) : ISeasonalAccessBadgeDraftStore
{
    public MemorySeasonalAccessBadgeDraftStore Inner { get; set; } = inner;
    public SeasonalAccessBadgeDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAccessBadgeDraft? Find(string code)
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

    public void Save(SeasonalAccessBadgeDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAccessBadgeDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal access badge drafts. Empty fields match all.</summary>
public sealed record SeasonalAccessBadgeDraftQuery(string Title, int MinStartsAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAccessBadgeDraft item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.StartsAt < MinStartsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAccessBadgeDraft> Apply(List<SeasonalAccessBadgeDraft> items)
    {
        var matched = Collections.Filter(items, (SeasonalAccessBadgeDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal access badge draft store.</summary>
public static class SeasonalAccessBadgeDraftStore
{
    public static ISeasonalAccessBadgeDraftStore NewMemorySeasonalAccessBadgeDraftStore()
    {
        return new MemorySeasonalAccessBadgeDraftStore(
            new Dictionary<string, SeasonalAccessBadgeDraft>(),
            new List<string>());
    }

    public static ISeasonalAccessBadgeDraftStore NewCachedSeasonalAccessBadgeDraftStore(MemorySeasonalAccessBadgeDraftStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAccessBadgeDraftStore(
            inner,
            SeasonalAccessBadgeDraftCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalAccessBadgeDraft> FindSeasonalAccessBadgeDrafts(ISeasonalAccessBadgeDraftStore store, SeasonalAccessBadgeDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
