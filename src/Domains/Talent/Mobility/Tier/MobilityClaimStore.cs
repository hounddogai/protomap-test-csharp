namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility claims by code.</summary>
public interface IMobilityClaimStore
{
    MobilityClaim? Find(string code);
    void Save(MobilityClaim item);
    bool Remove(string code);
    List<MobilityClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityClaimStore(Dictionary<string, MobilityClaim> items, List<string> order) : IMobilityClaimStore
{
    public Dictionary<string, MobilityClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityClaim item)
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

    public List<MobilityClaim> List()
    {
        var outValue = new List<MobilityClaim>();
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
public sealed class CachedMobilityClaimStore(MemoryMobilityClaimStore inner, MobilityClaimCache cache, int hits, int misses) : IMobilityClaimStore
{
    public MemoryMobilityClaimStore Inner { get; set; } = inner;
    public MobilityClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityClaim? Find(string code)
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

    public void Save(MobilityClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility claims. Empty fields match all.</summary>
public sealed record MobilityClaimQuery(string Locale, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityClaim item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<MobilityClaim> Apply(List<MobilityClaim> items)
    {
        var matched = Collections.Filter(items, (MobilityClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility claim store.</summary>
public static class MobilityClaimStore
{
    public static IMobilityClaimStore NewMemoryMobilityClaimStore()
    {
        return new MemoryMobilityClaimStore(new Dictionary<string, MobilityClaim>(), new List<string>());
    }

    public static IMobilityClaimStore NewCachedMobilityClaimStore(MemoryMobilityClaimStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityClaimStore(inner, MobilityClaimCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MobilityClaim> FindMobilityClaims(IMobilityClaimStore store, MobilityClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
