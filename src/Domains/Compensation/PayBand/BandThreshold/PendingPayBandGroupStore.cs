namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending pay band groups by code.</summary>
public interface IPendingPayBandGroupStore
{
    PendingPayBandGroup? Find(string code);
    void Save(PendingPayBandGroup item);
    bool Remove(string code);
    List<PendingPayBandGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPayBandGroupStore(Dictionary<string, PendingPayBandGroup> items, List<string> order) : IPendingPayBandGroupStore
{
    public Dictionary<string, PendingPayBandGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPayBandGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPayBandGroup item)
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

    public List<PendingPayBandGroup> List()
    {
        var outValue = new List<PendingPayBandGroup>();
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
public sealed class CachedPendingPayBandGroupStore(MemoryPendingPayBandGroupStore inner, PendingPayBandGroupCache cache, int hits, int misses) : IPendingPayBandGroupStore
{
    public MemoryPendingPayBandGroupStore Inner { get; set; } = inner;
    public PendingPayBandGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPayBandGroup? Find(string code)
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

    public void Save(PendingPayBandGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPayBandGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending pay band groups. Empty fields match all.</summary>
public sealed record PendingPayBandGroupQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPayBandGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Weight < MinWeight)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPayBandGroup> Apply(List<PendingPayBandGroup> items)
    {
        var matched = Collections.Filter(items, (PendingPayBandGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending pay band group store.</summary>
public static class PendingPayBandGroupStore
{
    public static IPendingPayBandGroupStore NewMemoryPendingPayBandGroupStore()
    {
        return new MemoryPendingPayBandGroupStore(new Dictionary<string, PendingPayBandGroup>(), new List<string>());
    }

    public static IPendingPayBandGroupStore NewCachedPendingPayBandGroupStore(MemoryPendingPayBandGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPayBandGroupStore(inner, PendingPayBandGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingPayBandGroup> FindPendingPayBandGroups(IPendingPayBandGroupStore store, PendingPayBandGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
