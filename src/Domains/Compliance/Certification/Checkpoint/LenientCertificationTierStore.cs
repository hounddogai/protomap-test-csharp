namespace Acme.Workforce.Compliance.Certification.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient certification tiers by code.</summary>
public interface ILenientCertificationTierStore
{
    LenientCertificationTier? Find(string code);
    void Save(LenientCertificationTier item);
    bool Remove(string code);
    List<LenientCertificationTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientCertificationTierStore(Dictionary<string, LenientCertificationTier> items, List<string> order) : ILenientCertificationTierStore
{
    public Dictionary<string, LenientCertificationTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientCertificationTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientCertificationTier item)
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

    public List<LenientCertificationTier> List()
    {
        var outValue = new List<LenientCertificationTier>();
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
public sealed class CachedLenientCertificationTierStore(MemoryLenientCertificationTierStore inner, LenientCertificationTierCache cache, int hits, int misses) : ILenientCertificationTierStore
{
    public MemoryLenientCertificationTierStore Inner { get; set; } = inner;
    public LenientCertificationTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientCertificationTier? Find(string code)
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

    public void Save(LenientCertificationTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientCertificationTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient certification tiers. Empty fields match all.</summary>
public sealed record LenientCertificationTierQuery(string Team, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(LenientCertificationTier item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<LenientCertificationTier> Apply(List<LenientCertificationTier> items)
    {
        var matched = Collections.Filter(items, (LenientCertificationTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient certification tier store.</summary>
public static class LenientCertificationTierStore
{
    public static ILenientCertificationTierStore NewMemoryLenientCertificationTierStore()
    {
        return new MemoryLenientCertificationTierStore(
            new Dictionary<string, LenientCertificationTier>(),
            new List<string>());
    }

    public static ILenientCertificationTierStore NewCachedLenientCertificationTierStore(MemoryLenientCertificationTierStore inner, int ttl, IClock clock)
    {
        return new CachedLenientCertificationTierStore(
            inner,
            LenientCertificationTierCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientCertificationTier> FindLenientCertificationTiers(ILenientCertificationTierStore store, LenientCertificationTierQuery query)
    {
        return query.Apply(store.List());
    }
}
