namespace Acme.Workforce.Talent.Nomination.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination rollups by code.</summary>
public interface INominationRollupStore
{
    NominationRollup? Find(string code);
    void Save(NominationRollup item);
    bool Remove(string code);
    List<NominationRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationRollupStore(Dictionary<string, NominationRollup> items, List<string> order) : INominationRollupStore
{
    public Dictionary<string, NominationRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationRollup item)
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

    public List<NominationRollup> List()
    {
        var outValue = new List<NominationRollup>();
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
public sealed class CachedNominationRollupStore(MemoryNominationRollupStore inner, NominationRollupCache cache, int hits, int misses) : INominationRollupStore
{
    public MemoryNominationRollupStore Inner { get; set; } = inner;
    public NominationRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationRollup? Find(string code)
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

    public void Save(NominationRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination rollups. Empty fields match all.</summary>
public sealed record NominationRollupQuery(string Label, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(NominationRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Ceiling < MinCeiling)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationRollup> Apply(List<NominationRollup> items)
    {
        var matched = Collections.Filter(items, (NominationRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination rollup store.</summary>
public static class NominationRollupStore
{
    public static INominationRollupStore NewMemoryNominationRollupStore()
    {
        return new MemoryNominationRollupStore(new Dictionary<string, NominationRollup>(), new List<string>());
    }

    public static INominationRollupStore NewCachedNominationRollupStore(MemoryNominationRollupStore inner, int ttl, IClock clock)
    {
        return new CachedNominationRollupStore(inner, NominationRollupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationRollup> FindNominationRollups(INominationRollupStore store, NominationRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
