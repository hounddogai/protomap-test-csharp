namespace Acme.Workforce.Payrollops.RetroPay.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal retro pay claims by code.</summary>
public interface ISeasonalRetroPayClaimStore
{
    SeasonalRetroPayClaim? Find(string code);
    void Save(SeasonalRetroPayClaim item);
    bool Remove(string code);
    List<SeasonalRetroPayClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalRetroPayClaimStore(Dictionary<string, SeasonalRetroPayClaim> items, List<string> order) : ISeasonalRetroPayClaimStore
{
    public Dictionary<string, SeasonalRetroPayClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalRetroPayClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalRetroPayClaim item)
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

    public List<SeasonalRetroPayClaim> List()
    {
        var outValue = new List<SeasonalRetroPayClaim>();
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
public sealed class CachedSeasonalRetroPayClaimStore(MemorySeasonalRetroPayClaimStore inner, SeasonalRetroPayClaimCache cache, int hits, int misses) : ISeasonalRetroPayClaimStore
{
    public MemorySeasonalRetroPayClaimStore Inner { get; set; } = inner;
    public SeasonalRetroPayClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalRetroPayClaim? Find(string code)
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

    public void Save(SeasonalRetroPayClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalRetroPayClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal retro pay claims. Empty fields match all.</summary>
public sealed record SeasonalRetroPayClaimQuery(string Name, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalRetroPayClaim item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalRetroPayClaim> Apply(List<SeasonalRetroPayClaim> items)
    {
        var matched = Collections.Filter(items, (SeasonalRetroPayClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal retro pay claim store.</summary>
public static class SeasonalRetroPayClaimStore
{
    public static ISeasonalRetroPayClaimStore NewMemorySeasonalRetroPayClaimStore()
    {
        return new MemorySeasonalRetroPayClaimStore(
            new Dictionary<string, SeasonalRetroPayClaim>(),
            new List<string>());
    }

    public static ISeasonalRetroPayClaimStore NewCachedSeasonalRetroPayClaimStore(MemorySeasonalRetroPayClaimStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalRetroPayClaimStore(
            inner,
            SeasonalRetroPayClaimCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<SeasonalRetroPayClaim> FindSeasonalRetroPayClaims(ISeasonalRetroPayClaimStore store, SeasonalRetroPayClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
