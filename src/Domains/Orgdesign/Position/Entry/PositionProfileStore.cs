namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps position profiles by code.</summary>
public interface IPositionProfileStore
{
    PositionProfile? Find(string code);
    void Save(PositionProfile item);
    bool Remove(string code);
    List<PositionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPositionProfileStore(Dictionary<string, PositionProfile> items, List<string> order) : IPositionProfileStore
{
    public Dictionary<string, PositionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PositionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PositionProfile item)
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

    public List<PositionProfile> List()
    {
        var outValue = new List<PositionProfile>();
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
public sealed class CachedPositionProfileStore(MemoryPositionProfileStore inner, PositionProfileCache cache, int hits, int misses) : IPositionProfileStore
{
    public MemoryPositionProfileStore Inner { get; set; } = inner;
    public PositionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PositionProfile? Find(string code)
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

    public void Save(PositionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PositionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects position profiles. Empty fields match all.</summary>
public sealed record PositionProfileQuery(string Segment, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(PositionProfile item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PositionProfile> Apply(List<PositionProfile> items)
    {
        var matched = Collections.Filter(items, (PositionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of position profile store.</summary>
public static class PositionProfileStore
{
    public static IPositionProfileStore NewMemoryPositionProfileStore()
    {
        return new MemoryPositionProfileStore(new Dictionary<string, PositionProfile>(), new List<string>());
    }

    public static IPositionProfileStore NewCachedPositionProfileStore(MemoryPositionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPositionProfileStore(inner, PositionProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PositionProfile> FindPositionProfiles(IPositionProfileStore store, PositionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
