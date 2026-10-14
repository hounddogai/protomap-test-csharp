namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending assignment tiers by code.</summary>
public interface IPendingAssignmentTierStore
{
    PendingAssignmentTier? Find(string code);
    void Save(PendingAssignmentTier item);
    bool Remove(string code);
    List<PendingAssignmentTier> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAssignmentTierStore(Dictionary<string, PendingAssignmentTier> items, List<string> order) : IPendingAssignmentTierStore
{
    public Dictionary<string, PendingAssignmentTier> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAssignmentTier? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAssignmentTier item)
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

    public List<PendingAssignmentTier> List()
    {
        var outValue = new List<PendingAssignmentTier>();
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
public sealed class CachedPendingAssignmentTierStore(MemoryPendingAssignmentTierStore inner, PendingAssignmentTierCache cache, int hits, int misses) : IPendingAssignmentTierStore
{
    public MemoryPendingAssignmentTierStore Inner { get; set; } = inner;
    public PendingAssignmentTierCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAssignmentTier? Find(string code)
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

    public void Save(PendingAssignmentTier item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAssignmentTier> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending assignment tiers. Empty fields match all.</summary>
public sealed record PendingAssignmentTierQuery(string Title, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAssignmentTier item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAssignmentTier> Apply(List<PendingAssignmentTier> items)
    {
        var matched = Collections.Filter(items, (PendingAssignmentTier candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending assignment tier store.</summary>
public static class PendingAssignmentTierStore
{
    public static IPendingAssignmentTierStore NewMemoryPendingAssignmentTierStore()
    {
        return new MemoryPendingAssignmentTierStore(
            new Dictionary<string, PendingAssignmentTier>(),
            new List<string>());
    }

    public static IPendingAssignmentTierStore NewCachedPendingAssignmentTierStore(MemoryPendingAssignmentTierStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAssignmentTierStore(inner, PendingAssignmentTierCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PendingAssignmentTier> FindPendingAssignmentTiers(IPendingAssignmentTierStore store, PendingAssignmentTierQuery query)
    {
        return query.Apply(store.List());
    }
}
