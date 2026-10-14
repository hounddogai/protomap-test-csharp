namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility groups by code.</summary>
public interface IMobilityGroupStore
{
    MobilityGroup? Find(string code);
    void Save(MobilityGroup item);
    bool Remove(string code);
    List<MobilityGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityGroupStore(Dictionary<string, MobilityGroup> items, List<string> order) : IMobilityGroupStore
{
    public Dictionary<string, MobilityGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityGroup item)
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

    public List<MobilityGroup> List()
    {
        var outValue = new List<MobilityGroup>();
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
public sealed class CachedMobilityGroupStore(MemoryMobilityGroupStore inner, MobilityGroupCache cache, int hits, int misses) : IMobilityGroupStore
{
    public MemoryMobilityGroupStore Inner { get; set; } = inner;
    public MobilityGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityGroup? Find(string code)
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

    public void Save(MobilityGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility groups. Empty fields match all.</summary>
public sealed record MobilityGroupQuery(string Name, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<MobilityGroup> Apply(List<MobilityGroup> items)
    {
        var matched = Collections.Filter(items, (MobilityGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility group store.</summary>
public static class MobilityGroupStore
{
    public static IMobilityGroupStore NewMemoryMobilityGroupStore()
    {
        return new MemoryMobilityGroupStore(new Dictionary<string, MobilityGroup>(), new List<string>());
    }

    public static IMobilityGroupStore NewCachedMobilityGroupStore(MemoryMobilityGroupStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityGroupStore(inner, MobilityGroupCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MobilityGroup> FindMobilityGroups(IMobilityGroupStore store, MobilityGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
