namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps reorg groups by code.</summary>
public interface IReorgGroupStore
{
    ReorgGroup? Find(string code);
    void Save(ReorgGroup item);
    bool Remove(string code);
    List<ReorgGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryReorgGroupStore(Dictionary<string, ReorgGroup> items, List<string> order) : IReorgGroupStore
{
    public Dictionary<string, ReorgGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ReorgGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ReorgGroup item)
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

    public List<ReorgGroup> List()
    {
        var outValue = new List<ReorgGroup>();
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
public sealed class CachedReorgGroupStore(MemoryReorgGroupStore inner, ReorgGroupCache cache, int hits, int misses) : IReorgGroupStore
{
    public MemoryReorgGroupStore Inner { get; set; } = inner;
    public ReorgGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ReorgGroup? Find(string code)
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

    public void Save(ReorgGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ReorgGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects reorg groups. Empty fields match all.</summary>
public sealed record ReorgGroupQuery(string Team, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(ReorgGroup item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<ReorgGroup> Apply(List<ReorgGroup> items)
    {
        var matched = Collections.Filter(items, (ReorgGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of reorg group store.</summary>
public static class ReorgGroupStore
{
    public static IReorgGroupStore NewMemoryReorgGroupStore()
    {
        return new MemoryReorgGroupStore(new Dictionary<string, ReorgGroup>(), new List<string>());
    }

    public static IReorgGroupStore NewCachedReorgGroupStore(MemoryReorgGroupStore inner, int ttl, IClock clock)
    {
        return new CachedReorgGroupStore(inner, ReorgGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ReorgGroup> FindReorgGroups(IReorgGroupStore store, ReorgGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
