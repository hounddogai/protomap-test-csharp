namespace Acme.Workforce.Absence.Policy.WeightedPolicyPreference;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps strict policy entries by code.</summary>
public interface IStrictPolicyEntryStore
{
    StrictPolicyEntry? Find(string code);
    void Save(StrictPolicyEntry item);
    bool Remove(string code);
    List<StrictPolicyEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryStrictPolicyEntryStore(Dictionary<string, StrictPolicyEntry> items, List<string> order) : IStrictPolicyEntryStore
{
    public Dictionary<string, StrictPolicyEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public StrictPolicyEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(StrictPolicyEntry item)
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

    public List<StrictPolicyEntry> List()
    {
        var outValue = new List<StrictPolicyEntry>();
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
public sealed class CachedStrictPolicyEntryStore(MemoryStrictPolicyEntryStore inner, StrictPolicyEntryCache cache, int hits, int misses) : IStrictPolicyEntryStore
{
    public MemoryStrictPolicyEntryStore Inner { get; set; } = inner;
    public StrictPolicyEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public StrictPolicyEntry? Find(string code)
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

    public void Save(StrictPolicyEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<StrictPolicyEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects strict policy entries. Empty fields match all.</summary>
public sealed record StrictPolicyEntryQuery(string Name, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(StrictPolicyEntry item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<StrictPolicyEntry> Apply(List<StrictPolicyEntry> items)
    {
        var matched = Collections.Filter(items, (StrictPolicyEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of strict policy entry store.</summary>
public static class StrictPolicyEntryStore
{
    public static IStrictPolicyEntryStore NewMemoryStrictPolicyEntryStore()
    {
        return new MemoryStrictPolicyEntryStore(new Dictionary<string, StrictPolicyEntry>(), new List<string>());
    }

    public static IStrictPolicyEntryStore NewCachedStrictPolicyEntryStore(MemoryStrictPolicyEntryStore inner, int ttl, IClock clock)
    {
        return new CachedStrictPolicyEntryStore(inner, StrictPolicyEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<StrictPolicyEntry> FindStrictPolicyEntries(IStrictPolicyEntryStore store, StrictPolicyEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
