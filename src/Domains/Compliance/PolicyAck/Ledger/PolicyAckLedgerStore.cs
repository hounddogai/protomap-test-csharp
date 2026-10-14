namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack ledgers by code.</summary>
public interface IPolicyAckLedgerStore
{
    PolicyAckLedger? Find(string code);
    void Save(PolicyAckLedger item);
    bool Remove(string code);
    List<PolicyAckLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckLedgerStore(Dictionary<string, PolicyAckLedger> items, List<string> order) : IPolicyAckLedgerStore
{
    public Dictionary<string, PolicyAckLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckLedger item)
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

    public List<PolicyAckLedger> List()
    {
        var outValue = new List<PolicyAckLedger>();
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
public sealed class CachedPolicyAckLedgerStore(MemoryPolicyAckLedgerStore inner, PolicyAckLedgerCache cache, int hits, int misses) : IPolicyAckLedgerStore
{
    public MemoryPolicyAckLedgerStore Inner { get; set; } = inner;
    public PolicyAckLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckLedger? Find(string code)
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

    public void Save(PolicyAckLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack ledgers. Empty fields match all.</summary>
public sealed record PolicyAckLedgerQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckLedger item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckLedger> Apply(List<PolicyAckLedger> items)
    {
        var matched = Collections.Filter(items, (PolicyAckLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack ledger store.</summary>
public static class PolicyAckLedgerStore
{
    public static IPolicyAckLedgerStore NewMemoryPolicyAckLedgerStore()
    {
        return new MemoryPolicyAckLedgerStore(new Dictionary<string, PolicyAckLedger>(), new List<string>());
    }

    public static IPolicyAckLedgerStore NewCachedPolicyAckLedgerStore(MemoryPolicyAckLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckLedgerStore(inner, PolicyAckLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PolicyAckLedger> FindPolicyAckLedgers(IPolicyAckLedgerStore store, PolicyAckLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
