namespace Acme.Workforce.Talent.Mobility.Preference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default mobility claims by code.</summary>
public interface IDefaultMobilityClaimStore
{
    DefaultMobilityClaim? Find(string code);
    void Save(DefaultMobilityClaim item);
    bool Remove(string code);
    List<DefaultMobilityClaim> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultMobilityClaimStore(Dictionary<string, DefaultMobilityClaim> items, List<string> order) : IDefaultMobilityClaimStore
{
    public Dictionary<string, DefaultMobilityClaim> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultMobilityClaim? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultMobilityClaim item)
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

    public List<DefaultMobilityClaim> List()
    {
        var outValue = new List<DefaultMobilityClaim>();
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
public sealed class CachedDefaultMobilityClaimStore(MemoryDefaultMobilityClaimStore inner, DefaultMobilityClaimCache cache, int hits, int misses) : IDefaultMobilityClaimStore
{
    public MemoryDefaultMobilityClaimStore Inner { get; set; } = inner;
    public DefaultMobilityClaimCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultMobilityClaim? Find(string code)
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

    public void Save(DefaultMobilityClaim item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultMobilityClaim> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default mobility claims. Empty fields match all.</summary>
public sealed record DefaultMobilityClaimQuery(string Team, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultMobilityClaim item)
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

    public List<DefaultMobilityClaim> Apply(List<DefaultMobilityClaim> items)
    {
        var matched = Collections.Filter(items, (DefaultMobilityClaim candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default mobility claim store.</summary>
public static class DefaultMobilityClaimStore
{
    public static IDefaultMobilityClaimStore NewMemoryDefaultMobilityClaimStore()
    {
        return new MemoryDefaultMobilityClaimStore(
            new Dictionary<string, DefaultMobilityClaim>(),
            new List<string>());
    }

    public static IDefaultMobilityClaimStore NewCachedDefaultMobilityClaimStore(MemoryDefaultMobilityClaimStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultMobilityClaimStore(inner, DefaultMobilityClaimCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<DefaultMobilityClaim> FindDefaultMobilityClaims(IDefaultMobilityClaimStore store, DefaultMobilityClaimQuery query)
    {
        return query.Apply(store.List());
    }
}
