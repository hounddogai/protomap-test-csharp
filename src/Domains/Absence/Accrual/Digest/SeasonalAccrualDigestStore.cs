namespace Acme.Workforce.Absence.Accrual.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps seasonal accrual digests by code.</summary>
public interface ISeasonalAccrualDigestStore
{
    SeasonalAccrualDigest? Find(string code);
    void Save(SeasonalAccrualDigest item);
    bool Remove(string code);
    List<SeasonalAccrualDigest> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySeasonalAccrualDigestStore(Dictionary<string, SeasonalAccrualDigest> items, List<string> order) : ISeasonalAccrualDigestStore
{
    public Dictionary<string, SeasonalAccrualDigest> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SeasonalAccrualDigest? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SeasonalAccrualDigest item)
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

    public List<SeasonalAccrualDigest> List()
    {
        var outValue = new List<SeasonalAccrualDigest>();
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
public sealed class CachedSeasonalAccrualDigestStore(MemorySeasonalAccrualDigestStore inner, SeasonalAccrualDigestCache cache, int hits, int misses) : ISeasonalAccrualDigestStore
{
    public MemorySeasonalAccrualDigestStore Inner { get; set; } = inner;
    public SeasonalAccrualDigestCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SeasonalAccrualDigest? Find(string code)
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

    public void Save(SeasonalAccrualDigest item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SeasonalAccrualDigest> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects seasonal accrual digests. Empty fields match all.</summary>
public sealed record SeasonalAccrualDigestQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(SeasonalAccrualDigest item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SeasonalAccrualDigest> Apply(List<SeasonalAccrualDigest> items)
    {
        var matched = Collections.Filter(items, (SeasonalAccrualDigest candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of seasonal accrual digest store.</summary>
public static class SeasonalAccrualDigestStore
{
    public static ISeasonalAccrualDigestStore NewMemorySeasonalAccrualDigestStore()
    {
        return new MemorySeasonalAccrualDigestStore(
            new Dictionary<string, SeasonalAccrualDigest>(),
            new List<string>());
    }

    public static ISeasonalAccrualDigestStore NewCachedSeasonalAccrualDigestStore(MemorySeasonalAccrualDigestStore inner, int ttl, IClock clock)
    {
        return new CachedSeasonalAccrualDigestStore(inner, SeasonalAccrualDigestCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<SeasonalAccrualDigest> FindSeasonalAccrualDigests(ISeasonalAccrualDigestStore store, SeasonalAccrualDigestQuery query)
    {
        return query.Apply(store.List());
    }
}
