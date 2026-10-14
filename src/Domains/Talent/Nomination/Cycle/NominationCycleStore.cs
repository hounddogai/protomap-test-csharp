namespace Acme.Workforce.Talent.Nomination.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination cycles by code.</summary>
public interface INominationCycleStore
{
    NominationCycle? Find(string code);
    void Save(NominationCycle item);
    bool Remove(string code);
    List<NominationCycle> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationCycleStore(Dictionary<string, NominationCycle> items, List<string> order) : INominationCycleStore
{
    public Dictionary<string, NominationCycle> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationCycle? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationCycle item)
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

    public List<NominationCycle> List()
    {
        var outValue = new List<NominationCycle>();
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
public sealed class CachedNominationCycleStore(MemoryNominationCycleStore inner, NominationCycleCache cache, int hits, int misses) : INominationCycleStore
{
    public MemoryNominationCycleStore Inner { get; set; } = inner;
    public NominationCycleCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationCycle? Find(string code)
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

    public void Save(NominationCycle item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationCycle> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination cycles. Empty fields match all.</summary>
public sealed record NominationCycleQuery(string Country, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(NominationCycle item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<NominationCycle> Apply(List<NominationCycle> items)
    {
        var matched = Collections.Filter(items, (NominationCycle candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination cycle store.</summary>
public static class NominationCycleStore
{
    public static INominationCycleStore NewMemoryNominationCycleStore()
    {
        return new MemoryNominationCycleStore(new Dictionary<string, NominationCycle>(), new List<string>());
    }

    public static INominationCycleStore NewCachedNominationCycleStore(MemoryNominationCycleStore inner, int ttl, IClock clock)
    {
        return new CachedNominationCycleStore(inner, NominationCycleCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<NominationCycle> FindNominationCycles(INominationCycleStore store, NominationCycleQuery query)
    {
        return query.Apply(store.List());
    }
}
