namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling access badge profiles by code.</summary>
public interface IRollingAccessBadgeProfileStore
{
    RollingAccessBadgeProfile? Find(string code);
    void Save(RollingAccessBadgeProfile item);
    bool Remove(string code);
    List<RollingAccessBadgeProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingAccessBadgeProfileStore(Dictionary<string, RollingAccessBadgeProfile> items, List<string> order) : IRollingAccessBadgeProfileStore
{
    public Dictionary<string, RollingAccessBadgeProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingAccessBadgeProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingAccessBadgeProfile item)
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

    public List<RollingAccessBadgeProfile> List()
    {
        var outValue = new List<RollingAccessBadgeProfile>();
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
public sealed class CachedRollingAccessBadgeProfileStore(MemoryRollingAccessBadgeProfileStore inner, RollingAccessBadgeProfileCache cache, int hits, int misses) : IRollingAccessBadgeProfileStore
{
    public MemoryRollingAccessBadgeProfileStore Inner { get; set; } = inner;
    public RollingAccessBadgeProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingAccessBadgeProfile? Find(string code)
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

    public void Save(RollingAccessBadgeProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingAccessBadgeProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling access badge profiles. Empty fields match all.</summary>
public sealed record RollingAccessBadgeProfileQuery(string Label, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RollingAccessBadgeProfile item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingAccessBadgeProfile> Apply(List<RollingAccessBadgeProfile> items)
    {
        var matched = Collections.Filter(items, (RollingAccessBadgeProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling access badge profile store.</summary>
public static class RollingAccessBadgeProfileStore
{
    public static IRollingAccessBadgeProfileStore NewMemoryRollingAccessBadgeProfileStore()
    {
        return new MemoryRollingAccessBadgeProfileStore(
            new Dictionary<string, RollingAccessBadgeProfile>(),
            new List<string>());
    }

    public static IRollingAccessBadgeProfileStore NewCachedRollingAccessBadgeProfileStore(MemoryRollingAccessBadgeProfileStore inner, int ttl, IClock clock)
    {
        return new CachedRollingAccessBadgeProfileStore(
            inner,
            RollingAccessBadgeProfileCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RollingAccessBadgeProfile> FindRollingAccessBadgeProfiles(IRollingAccessBadgeProfileStore store, RollingAccessBadgeProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
