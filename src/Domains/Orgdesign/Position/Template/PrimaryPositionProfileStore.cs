namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary position profiles by code.</summary>
public interface IPrimaryPositionProfileStore
{
    PrimaryPositionProfile? Find(string code);
    void Save(PrimaryPositionProfile item);
    bool Remove(string code);
    List<PrimaryPositionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPositionProfileStore(Dictionary<string, PrimaryPositionProfile> items, List<string> order) : IPrimaryPositionProfileStore
{
    public Dictionary<string, PrimaryPositionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPositionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPositionProfile item)
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

    public List<PrimaryPositionProfile> List()
    {
        var outValue = new List<PrimaryPositionProfile>();
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
public sealed class CachedPrimaryPositionProfileStore(MemoryPrimaryPositionProfileStore inner, PrimaryPositionProfileCache cache, int hits, int misses) : IPrimaryPositionProfileStore
{
    public MemoryPrimaryPositionProfileStore Inner { get; set; } = inner;
    public PrimaryPositionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPositionProfile? Find(string code)
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

    public void Save(PrimaryPositionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPositionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary position profiles. Empty fields match all.</summary>
public sealed record PrimaryPositionProfileQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPositionProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPositionProfile> Apply(List<PrimaryPositionProfile> items)
    {
        var matched = Collections.Filter(items, (PrimaryPositionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary position profile store.</summary>
public static class PrimaryPositionProfileStore
{
    public static IPrimaryPositionProfileStore NewMemoryPrimaryPositionProfileStore()
    {
        return new MemoryPrimaryPositionProfileStore(
            new Dictionary<string, PrimaryPositionProfile>(),
            new List<string>());
    }

    public static IPrimaryPositionProfileStore NewCachedPrimaryPositionProfileStore(MemoryPrimaryPositionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPositionProfileStore(
            inner,
            PrimaryPositionProfileCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPositionProfile> FindPrimaryPositionProfiles(IPrimaryPositionProfileStore store, PrimaryPositionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
