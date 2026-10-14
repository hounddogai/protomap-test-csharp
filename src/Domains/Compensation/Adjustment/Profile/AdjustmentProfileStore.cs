namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps adjustment profiles by code.</summary>
public interface IAdjustmentProfileStore
{
    AdjustmentProfile? Find(string code);
    void Save(AdjustmentProfile item);
    bool Remove(string code);
    List<AdjustmentProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAdjustmentProfileStore(Dictionary<string, AdjustmentProfile> items, List<string> order) : IAdjustmentProfileStore
{
    public Dictionary<string, AdjustmentProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AdjustmentProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AdjustmentProfile item)
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

    public List<AdjustmentProfile> List()
    {
        var outValue = new List<AdjustmentProfile>();
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
public sealed class CachedAdjustmentProfileStore(MemoryAdjustmentProfileStore inner, AdjustmentProfileCache cache, int hits, int misses) : IAdjustmentProfileStore
{
    public MemoryAdjustmentProfileStore Inner { get; set; } = inner;
    public AdjustmentProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AdjustmentProfile? Find(string code)
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

    public void Save(AdjustmentProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AdjustmentProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects adjustment profiles. Empty fields match all.</summary>
public sealed record AdjustmentProfileQuery(string Team, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(AdjustmentProfile item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AdjustmentProfile> Apply(List<AdjustmentProfile> items)
    {
        var matched = Collections.Filter(items, (AdjustmentProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of adjustment profile store.</summary>
public static class AdjustmentProfileStore
{
    public static IAdjustmentProfileStore NewMemoryAdjustmentProfileStore()
    {
        return new MemoryAdjustmentProfileStore(new Dictionary<string, AdjustmentProfile>(), new List<string>());
    }

    public static IAdjustmentProfileStore NewCachedAdjustmentProfileStore(MemoryAdjustmentProfileStore inner, int ttl, IClock clock)
    {
        return new CachedAdjustmentProfileStore(inner, AdjustmentProfileCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AdjustmentProfile> FindAdjustmentProfiles(IAdjustmentProfileStore store, AdjustmentProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
