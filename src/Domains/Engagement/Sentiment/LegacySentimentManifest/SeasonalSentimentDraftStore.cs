namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal sentiment drafts by code.</summary>
public interface ISeasonalSentimentDraftStore
{
    SeasonalSentimentDraft? Find(string code);
    void Save(SeasonalSentimentDraft item);
    bool Remove(string code);
    List<SeasonalSentimentDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalSentimentDraftStore(Dictionary<string, SeasonalSentimentDraft> items, List<string> order) : ISeasonalSentimentDraftStore
{
    public Dictionary<string, SeasonalSentimentDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalSentimentDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalSentimentDraft item)
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

    public List<SeasonalSentimentDraft> List()
    {
        var outValue = new List<SeasonalSentimentDraft>();
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
public sealed class CachedSeasonalSentimentDraftStore(MemorySeasonalSentimentDraftStore inner, SeasonalSentimentDraftCache cache, int hits, int misses) : ISeasonalSentimentDraftStore
{
    public MemorySeasonalSentimentDraftStore Inner { get; set; } = inner;
    public SeasonalSentimentDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalSentimentDraft? Find(string code)
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

    public void Save(SeasonalSentimentDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalSentimentDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal sentiment drafts. Empty fields match all.</summary>
public sealed record SeasonalSentimentDraftQuery(string Label, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalSentimentDraft item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalSentimentDraft> Apply(List<SeasonalSentimentDraft> items)
    {
        var matched = Collections.Filter(items, (SeasonalSentimentDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal sentiment draft store.</summary>
public static class SeasonalSentimentDraftStore
{
    public static ISeasonalSentimentDraftStore NewMemorySeasonalSentimentDraftStore()
    {
        return new MemorySeasonalSentimentDraftStore(
            new Dictionary<string, SeasonalSentimentDraft>(),
            new List<string>());
    }

    public static ISeasonalSentimentDraftStore NewCachedSeasonalSentimentDraftStore(MemorySeasonalSentimentDraftStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalSentimentDraftStore(
            inner,
            SeasonalSentimentDraftCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalSentimentDraft> FindSeasonalSentimentDrafts(ISeasonalSentimentDraftStore store, SeasonalSentimentDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
