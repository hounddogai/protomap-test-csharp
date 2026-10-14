namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal feedback claims by code.</summary>
public interface ISeasonalFeedbackClaimStore
{
    SeasonalFeedbackClaim? Find(string code);
    void Save(SeasonalFeedbackClaim item);
    bool Remove(string code);
    List<SeasonalFeedbackClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalFeedbackClaimStore(Dictionary<string, SeasonalFeedbackClaim> items, List<string> order) : ISeasonalFeedbackClaimStore
{
    public Dictionary<string, SeasonalFeedbackClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalFeedbackClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalFeedbackClaim item)
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

    public List<SeasonalFeedbackClaim> List()
    {
        var outValue = new List<SeasonalFeedbackClaim>();
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
public sealed class CachedSeasonalFeedbackClaimStore(MemorySeasonalFeedbackClaimStore inner, SeasonalFeedbackClaimCache cache, int hits, int misses) : ISeasonalFeedbackClaimStore
{
    public MemorySeasonalFeedbackClaimStore Inner { get; set; } = inner;
    public SeasonalFeedbackClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalFeedbackClaim? Find(string code)
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

    public void Save(SeasonalFeedbackClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalFeedbackClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal feedback claims. Empty fields match all.</summary>
public sealed record SeasonalFeedbackClaimQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalFeedbackClaim item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalFeedbackClaim> Apply(List<SeasonalFeedbackClaim> items)
    {
        var matched = Collections.Filter(items, (SeasonalFeedbackClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal feedback claim store.</summary>
public static class SeasonalFeedbackClaimStore
{
    public static ISeasonalFeedbackClaimStore NewMemorySeasonalFeedbackClaimStore()
    {
        return new MemorySeasonalFeedbackClaimStore(
            new Dictionary<string, SeasonalFeedbackClaim>(),
            new List<string>());
    }

    public static ISeasonalFeedbackClaimStore NewCachedSeasonalFeedbackClaimStore(MemorySeasonalFeedbackClaimStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalFeedbackClaimStore(inner, SeasonalFeedbackClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<SeasonalFeedbackClaim> FindSeasonalFeedbackClaims(ISeasonalFeedbackClaimStore store, SeasonalFeedbackClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
