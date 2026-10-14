namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict bonus pool groups by code.</summary>
public interface IStrictBonusPoolGroupStore
{
    StrictBonusPoolGroup? Find(string code);
    void Save(StrictBonusPoolGroup item);
    bool Remove(string code);
    List<StrictBonusPoolGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictBonusPoolGroupStore(Dictionary<string, StrictBonusPoolGroup> items, List<string> order) : IStrictBonusPoolGroupStore
{
    public Dictionary<string, StrictBonusPoolGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictBonusPoolGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictBonusPoolGroup item)
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

    public List<StrictBonusPoolGroup> List()
    {
        var outValue = new List<StrictBonusPoolGroup>();
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
public sealed class CachedStrictBonusPoolGroupStore(MemoryStrictBonusPoolGroupStore inner, StrictBonusPoolGroupCache cache, int hits, int misses) : IStrictBonusPoolGroupStore
{
    public MemoryStrictBonusPoolGroupStore Inner { get; set; } = inner;
    public StrictBonusPoolGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictBonusPoolGroup? Find(string code)
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

    public void Save(StrictBonusPoolGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictBonusPoolGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict bonus pool groups. Empty fields match all.</summary>
public sealed record StrictBonusPoolGroupQuery(string Channel, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(StrictBonusPoolGroup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictBonusPoolGroup> Apply(List<StrictBonusPoolGroup> items)
    {
        var matched = Collections.Filter(items, (StrictBonusPoolGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict bonus pool group store.</summary>
public static class StrictBonusPoolGroupStore
{
    public static IStrictBonusPoolGroupStore NewMemoryStrictBonusPoolGroupStore()
    {
        return new MemoryStrictBonusPoolGroupStore(
            new Dictionary<string, StrictBonusPoolGroup>(),
            new List<string>());
    }

    public static IStrictBonusPoolGroupStore NewCachedStrictBonusPoolGroupStore(MemoryStrictBonusPoolGroupStore inner, int ttl, IClock clock)
    {
        return new CachedStrictBonusPoolGroupStore(inner, StrictBonusPoolGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<StrictBonusPoolGroup> FindStrictBonusPoolGroups(IStrictBonusPoolGroupStore store, StrictBonusPoolGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
