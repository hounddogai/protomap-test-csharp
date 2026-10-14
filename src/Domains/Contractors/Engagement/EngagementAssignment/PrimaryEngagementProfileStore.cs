namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary engagement profiles by code.</summary>
public interface IPrimaryEngagementProfileStore
{
    PrimaryEngagementProfile? Find(string code);
    void Save(PrimaryEngagementProfile item);
    bool Remove(string code);
    List<PrimaryEngagementProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryEngagementProfileStore(Dictionary<string, PrimaryEngagementProfile> items, List<string> order) : IPrimaryEngagementProfileStore
{
    public Dictionary<string, PrimaryEngagementProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryEngagementProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryEngagementProfile item)
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

    public List<PrimaryEngagementProfile> List()
    {
        var outValue = new List<PrimaryEngagementProfile>();
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
public sealed class CachedPrimaryEngagementProfileStore(MemoryPrimaryEngagementProfileStore inner, PrimaryEngagementProfileCache cache, int hits, int misses) : IPrimaryEngagementProfileStore
{
    public MemoryPrimaryEngagementProfileStore Inner { get; set; } = inner;
    public PrimaryEngagementProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryEngagementProfile? Find(string code)
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

    public void Save(PrimaryEngagementProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryEngagementProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary engagement profiles. Empty fields match all.</summary>
public sealed record PrimaryEngagementProfileQuery(string Source, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryEngagementProfile item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryEngagementProfile> Apply(List<PrimaryEngagementProfile> items)
    {
        var matched = Collections.Filter(items, (PrimaryEngagementProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary engagement profile store.</summary>
public static class PrimaryEngagementProfileStore
{
    public static IPrimaryEngagementProfileStore NewMemoryPrimaryEngagementProfileStore()
    {
        return new MemoryPrimaryEngagementProfileStore(
            new Dictionary<string, PrimaryEngagementProfile>(),
            new List<string>());
    }

    public static IPrimaryEngagementProfileStore NewCachedPrimaryEngagementProfileStore(MemoryPrimaryEngagementProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryEngagementProfileStore(
            inner,
            PrimaryEngagementProfileCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryEngagementProfile> FindPrimaryEngagementProfiles(IPrimaryEngagementProfileStore store, PrimaryEngagementProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
