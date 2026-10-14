namespace Acme.Workforce.Absence.Policy.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy records by code.</summary>
public interface IPolicyRecordStore
{
    PolicyRecord? Find(string code);
    void Save(PolicyRecord item);
    bool Remove(string code);
    List<PolicyRecord> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyRecordStore(Dictionary<string, PolicyRecord> items, List<string> order) : IPolicyRecordStore
{
    public Dictionary<string, PolicyRecord> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyRecord? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyRecord item)
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

    public List<PolicyRecord> List()
    {
        var outValue = new List<PolicyRecord>();
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
public sealed class CachedPolicyRecordStore(MemoryPolicyRecordStore inner, PolicyRecordCache cache, int hits, int misses) : IPolicyRecordStore
{
    public MemoryPolicyRecordStore Inner { get; set; } = inner;
    public PolicyRecordCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyRecord? Find(string code)
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

    public void Save(PolicyRecord item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyRecord> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy records. Empty fields match all.</summary>
public sealed record PolicyRecordQuery(string Label, double MinWeight, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyRecord item)
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

    public List<PolicyRecord> Apply(List<PolicyRecord> items)
    {
        var matched = Collections.Filter(items, (PolicyRecord candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy record store.</summary>
public static class PolicyRecordStore
{
    public static IPolicyRecordStore NewMemoryPolicyRecordStore()
    {
        return new MemoryPolicyRecordStore(new Dictionary<string, PolicyRecord>(), new List<string>());
    }

    public static IPolicyRecordStore NewCachedPolicyRecordStore(MemoryPolicyRecordStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyRecordStore(inner, PolicyRecordCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PolicyRecord> FindPolicyRecords(IPolicyRecordStore store, PolicyRecordQuery query)
    {
        return query.Apply(store.List());
    }
}
