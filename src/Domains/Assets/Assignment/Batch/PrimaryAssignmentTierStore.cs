namespace Acme.Workforce.Assets.Assignment.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary assignment tiers by code.</summary>
public interface IPrimaryAssignmentTierStore
{
    PrimaryAssignmentTier? Find(string code);
    void Save(PrimaryAssignmentTier item);
    bool Remove(string code);
    List<PrimaryAssignmentTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAssignmentTierStore(Dictionary<string, PrimaryAssignmentTier> items, List<string> order) : IPrimaryAssignmentTierStore
{
    public Dictionary<string, PrimaryAssignmentTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAssignmentTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAssignmentTier item)
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

    public List<PrimaryAssignmentTier> List()
    {
        var outValue = new List<PrimaryAssignmentTier>();
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
public sealed class CachedPrimaryAssignmentTierStore(MemoryPrimaryAssignmentTierStore inner, PrimaryAssignmentTierCache cache, int hits, int misses) : IPrimaryAssignmentTierStore
{
    public MemoryPrimaryAssignmentTierStore Inner { get; set; } = inner;
    public PrimaryAssignmentTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAssignmentTier? Find(string code)
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

    public void Save(PrimaryAssignmentTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAssignmentTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary assignment tiers. Empty fields match all.</summary>
public sealed record PrimaryAssignmentTierQuery(string Country, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAssignmentTier item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAssignmentTier> Apply(List<PrimaryAssignmentTier> items)
    {
        var matched = Collections.Filter(items, (PrimaryAssignmentTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary assignment tier store.</summary>
public static class PrimaryAssignmentTierStore
{
    public static IPrimaryAssignmentTierStore NewMemoryPrimaryAssignmentTierStore()
    {
        return new MemoryPrimaryAssignmentTierStore(
            new Dictionary<string, PrimaryAssignmentTier>(),
            new List<string>());
    }

    public static IPrimaryAssignmentTierStore NewCachedPrimaryAssignmentTierStore(MemoryPrimaryAssignmentTierStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAssignmentTierStore(
            inner,
            PrimaryAssignmentTierCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAssignmentTier> FindPrimaryAssignmentTiers(IPrimaryAssignmentTierStore store, PrimaryAssignmentTierQuery query)
    {
        return query.Apply(store.List());
    }
}
