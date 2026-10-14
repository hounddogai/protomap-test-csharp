namespace Acme.Workforce.Talent.Nomination.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination adjustments by code.</summary>
public interface INominationAdjustmentStore
{
    NominationAdjustment? Find(string code);
    void Save(NominationAdjustment item);
    bool Remove(string code);
    List<NominationAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationAdjustmentStore(Dictionary<string, NominationAdjustment> items, List<string> order) : INominationAdjustmentStore
{
    public Dictionary<string, NominationAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationAdjustment item)
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

    public List<NominationAdjustment> List()
    {
        var outValue = new List<NominationAdjustment>();
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
public sealed class CachedNominationAdjustmentStore(MemoryNominationAdjustmentStore inner, NominationAdjustmentCache cache, int hits, int misses) : INominationAdjustmentStore
{
    public MemoryNominationAdjustmentStore Inner { get; set; } = inner;
    public NominationAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationAdjustment? Find(string code)
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

    public void Save(NominationAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination adjustments. Empty fields match all.</summary>
public sealed record NominationAdjustmentQuery(string Region, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(NominationAdjustment item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<NominationAdjustment> Apply(List<NominationAdjustment> items)
    {
        var matched = Collections.Filter(items, (NominationAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination adjustment store.</summary>
public static class NominationAdjustmentStore
{
    public static INominationAdjustmentStore NewMemoryNominationAdjustmentStore()
    {
        return new MemoryNominationAdjustmentStore(
            new Dictionary<string, NominationAdjustment>(),
            new List<string>());
    }

    public static INominationAdjustmentStore NewCachedNominationAdjustmentStore(MemoryNominationAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedNominationAdjustmentStore(inner, NominationAdjustmentCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<NominationAdjustment> FindNominationAdjustments(INominationAdjustmentStore store, NominationAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
