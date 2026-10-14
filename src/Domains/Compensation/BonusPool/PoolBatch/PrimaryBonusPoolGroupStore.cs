namespace Acme.Workforce.Compensation.BonusPool.PoolBatch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary bonus pool groups by code.</summary>
public interface IPrimaryBonusPoolGroupStore
{
    PrimaryBonusPoolGroup? Find(string code);
    void Save(PrimaryBonusPoolGroup item);
    bool Remove(string code);
    List<PrimaryBonusPoolGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryBonusPoolGroupStore(Dictionary<string, PrimaryBonusPoolGroup> items, List<string> order) : IPrimaryBonusPoolGroupStore
{
    public Dictionary<string, PrimaryBonusPoolGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryBonusPoolGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryBonusPoolGroup item)
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

    public List<PrimaryBonusPoolGroup> List()
    {
        var outValue = new List<PrimaryBonusPoolGroup>();
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
public sealed class CachedPrimaryBonusPoolGroupStore(MemoryPrimaryBonusPoolGroupStore inner, PrimaryBonusPoolGroupCache cache, int hits, int misses) : IPrimaryBonusPoolGroupStore
{
    public MemoryPrimaryBonusPoolGroupStore Inner { get; set; } = inner;
    public PrimaryBonusPoolGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryBonusPoolGroup? Find(string code)
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

    public void Save(PrimaryBonusPoolGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryBonusPoolGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary bonus pool groups. Empty fields match all.</summary>
public sealed record PrimaryBonusPoolGroupQuery(string Segment, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryBonusPoolGroup item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryBonusPoolGroup> Apply(List<PrimaryBonusPoolGroup> items)
    {
        var matched = Collections.Filter(items, (PrimaryBonusPoolGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary bonus pool group store.</summary>
public static class PrimaryBonusPoolGroupStore
{
    public static IPrimaryBonusPoolGroupStore NewMemoryPrimaryBonusPoolGroupStore()
    {
        return new MemoryPrimaryBonusPoolGroupStore(
            new Dictionary<string, PrimaryBonusPoolGroup>(),
            new List<string>());
    }

    public static IPrimaryBonusPoolGroupStore NewCachedPrimaryBonusPoolGroupStore(MemoryPrimaryBonusPoolGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryBonusPoolGroupStore(inner, PrimaryBonusPoolGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PrimaryBonusPoolGroup> FindPrimaryBonusPoolGroups(IPrimaryBonusPoolGroupStore store, PrimaryBonusPoolGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
