namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps headcount groups by code.</summary>
public interface IHeadcountGroupStore
{
    HeadcountGroup? Find(string code);
    void Save(HeadcountGroup item);
    bool Remove(string code);
    List<HeadcountGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryHeadcountGroupStore(Dictionary<string, HeadcountGroup> items, List<string> order) : IHeadcountGroupStore
{
    public Dictionary<string, HeadcountGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public HeadcountGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(HeadcountGroup item)
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

    public List<HeadcountGroup> List()
    {
        var outValue = new List<HeadcountGroup>();
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
public sealed class CachedHeadcountGroupStore(MemoryHeadcountGroupStore inner, HeadcountGroupCache cache, int hits, int misses) : IHeadcountGroupStore
{
    public MemoryHeadcountGroupStore Inner { get; set; } = inner;
    public HeadcountGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public HeadcountGroup? Find(string code)
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

    public void Save(HeadcountGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<HeadcountGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects headcount groups. Empty fields match all.</summary>
public sealed record HeadcountGroupQuery(string Name, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(HeadcountGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<HeadcountGroup> Apply(List<HeadcountGroup> items)
    {
        var matched = Collections.Filter(items, (HeadcountGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of headcount group store.</summary>
public static class HeadcountGroupStore
{
    public static IHeadcountGroupStore NewMemoryHeadcountGroupStore()
    {
        return new MemoryHeadcountGroupStore(new Dictionary<string, HeadcountGroup>(), new List<string>());
    }

    public static IHeadcountGroupStore NewCachedHeadcountGroupStore(MemoryHeadcountGroupStore inner, int ttl, IClock clock)
    {
        return new CachedHeadcountGroupStore(inner, HeadcountGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<HeadcountGroup> FindHeadcountGroups(IHeadcountGroupStore store, HeadcountGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
