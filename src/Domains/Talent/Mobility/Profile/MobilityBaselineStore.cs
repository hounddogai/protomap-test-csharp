namespace Acme.Workforce.Talent.Mobility.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps mobility baselines by code.</summary>
public interface IMobilityBaselineStore
{
    MobilityBaseline? Find(string code);
    void Save(MobilityBaseline item);
    bool Remove(string code);
    List<MobilityBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryMobilityBaselineStore(Dictionary<string, MobilityBaseline> items, List<string> order) : IMobilityBaselineStore
{
    public Dictionary<string, MobilityBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public MobilityBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(MobilityBaseline item)
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

    public List<MobilityBaseline> List()
    {
        var outValue = new List<MobilityBaseline>();
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
public sealed class CachedMobilityBaselineStore(MemoryMobilityBaselineStore inner, MobilityBaselineCache cache, int hits, int misses) : IMobilityBaselineStore
{
    public MemoryMobilityBaselineStore Inner { get; set; } = inner;
    public MobilityBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public MobilityBaseline? Find(string code)
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

    public void Save(MobilityBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<MobilityBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects mobility baselines. Empty fields match all.</summary>
public sealed record MobilityBaselineQuery(string Title, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(MobilityBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<MobilityBaseline> Apply(List<MobilityBaseline> items)
    {
        var matched = Collections.Filter(items, (MobilityBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of mobility baseline store.</summary>
public static class MobilityBaselineStore
{
    public static IMobilityBaselineStore NewMemoryMobilityBaselineStore()
    {
        return new MemoryMobilityBaselineStore(new Dictionary<string, MobilityBaseline>(), new List<string>());
    }

    public static IMobilityBaselineStore NewCachedMobilityBaselineStore(MemoryMobilityBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedMobilityBaselineStore(inner, MobilityBaselineCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<MobilityBaseline> FindMobilityBaselines(IMobilityBaselineStore store, MobilityBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
