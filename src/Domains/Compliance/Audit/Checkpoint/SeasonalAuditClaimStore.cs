namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal audit claims by code.</summary>
public interface ISeasonalAuditClaimStore
{
    SeasonalAuditClaim? Find(string code);
    void Save(SeasonalAuditClaim item);
    bool Remove(string code);
    List<SeasonalAuditClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAuditClaimStore(Dictionary<string, SeasonalAuditClaim> items, List<string> order) : ISeasonalAuditClaimStore
{
    public Dictionary<string, SeasonalAuditClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAuditClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAuditClaim item)
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

    public List<SeasonalAuditClaim> List()
    {
        var outValue = new List<SeasonalAuditClaim>();
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
public sealed class CachedSeasonalAuditClaimStore(MemorySeasonalAuditClaimStore inner, SeasonalAuditClaimCache cache, int hits, int misses) : ISeasonalAuditClaimStore
{
    public MemorySeasonalAuditClaimStore Inner { get; set; } = inner;
    public SeasonalAuditClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAuditClaim? Find(string code)
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

    public void Save(SeasonalAuditClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAuditClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal audit claims. Empty fields match all.</summary>
public sealed record SeasonalAuditClaimQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAuditClaim item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAuditClaim> Apply(List<SeasonalAuditClaim> items)
    {
        var matched = Collections.Filter(items, (SeasonalAuditClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal audit claim store.</summary>
public static class SeasonalAuditClaimStore
{
    public static ISeasonalAuditClaimStore NewMemorySeasonalAuditClaimStore()
    {
        return new MemorySeasonalAuditClaimStore(new Dictionary<string, SeasonalAuditClaim>(), new List<string>());
    }

    public static ISeasonalAuditClaimStore NewCachedSeasonalAuditClaimStore(MemorySeasonalAuditClaimStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAuditClaimStore(inner, SeasonalAuditClaimCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<SeasonalAuditClaim> FindSeasonalAuditClaims(ISeasonalAuditClaimStore store, SeasonalAuditClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
