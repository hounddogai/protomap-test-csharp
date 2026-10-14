namespace Acme.Workforce.Engagement.TownHall.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal town hall claims by code.</summary>
public interface ISeasonalTownHallClaimStore
{
    SeasonalTownHallClaim? Find(string code);
    void Save(SeasonalTownHallClaim item);
    bool Remove(string code);
    List<SeasonalTownHallClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalTownHallClaimStore(Dictionary<string, SeasonalTownHallClaim> items, List<string> order) : ISeasonalTownHallClaimStore
{
    public Dictionary<string, SeasonalTownHallClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalTownHallClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalTownHallClaim item)
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

    public List<SeasonalTownHallClaim> List()
    {
        var outValue = new List<SeasonalTownHallClaim>();
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
public sealed class CachedSeasonalTownHallClaimStore(MemorySeasonalTownHallClaimStore inner, SeasonalTownHallClaimCache cache, int hits, int misses) : ISeasonalTownHallClaimStore
{
    public MemorySeasonalTownHallClaimStore Inner { get; set; } = inner;
    public SeasonalTownHallClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalTownHallClaim? Find(string code)
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

    public void Save(SeasonalTownHallClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalTownHallClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal town hall claims. Empty fields match all.</summary>
public sealed record SeasonalTownHallClaimQuery(string Source, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalTownHallClaim item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalTownHallClaim> Apply(List<SeasonalTownHallClaim> items)
    {
        var matched = Collections.Filter(items, (SeasonalTownHallClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal town hall claim store.</summary>
public static class SeasonalTownHallClaimStore
{
    public static ISeasonalTownHallClaimStore NewMemorySeasonalTownHallClaimStore()
    {
        return new MemorySeasonalTownHallClaimStore(
            new Dictionary<string, SeasonalTownHallClaim>(),
            new List<string>());
    }

    public static ISeasonalTownHallClaimStore NewCachedSeasonalTownHallClaimStore(MemorySeasonalTownHallClaimStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalTownHallClaimStore(
            inner,
            SeasonalTownHallClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalTownHallClaim> FindSeasonalTownHallClaims(ISeasonalTownHallClaimStore store, SeasonalTownHallClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
