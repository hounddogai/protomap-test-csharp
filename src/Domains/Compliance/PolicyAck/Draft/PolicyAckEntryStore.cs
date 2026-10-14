namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack entries by code.</summary>
public interface IPolicyAckEntryStore
{
    PolicyAckEntry? Find(string code);
    void Save(PolicyAckEntry item);
    bool Remove(string code);
    List<PolicyAckEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckEntryStore(Dictionary<string, PolicyAckEntry> items, List<string> order) : IPolicyAckEntryStore
{
    public Dictionary<string, PolicyAckEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckEntry item)
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

    public List<PolicyAckEntry> List()
    {
        var outValue = new List<PolicyAckEntry>();
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
public sealed class CachedPolicyAckEntryStore(MemoryPolicyAckEntryStore inner, PolicyAckEntryCache cache, int hits, int misses) : IPolicyAckEntryStore
{
    public MemoryPolicyAckEntryStore Inner { get; set; } = inner;
    public PolicyAckEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckEntry? Find(string code)
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

    public void Save(PolicyAckEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack entries. Empty fields match all.</summary>
public sealed record PolicyAckEntryQuery(string Source, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckEntry item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckEntry> Apply(List<PolicyAckEntry> items)
    {
        var matched = Collections.Filter(items, (PolicyAckEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack entry store.</summary>
public static class PolicyAckEntryStore
{
    public static IPolicyAckEntryStore NewMemoryPolicyAckEntryStore()
    {
        return new MemoryPolicyAckEntryStore(new Dictionary<string, PolicyAckEntry>(), new List<string>());
    }

    public static IPolicyAckEntryStore NewCachedPolicyAckEntryStore(MemoryPolicyAckEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckEntryStore(inner, PolicyAckEntryCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyAckEntry> FindPolicyAckEntries(IPolicyAckEntryStore store, PolicyAckEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
