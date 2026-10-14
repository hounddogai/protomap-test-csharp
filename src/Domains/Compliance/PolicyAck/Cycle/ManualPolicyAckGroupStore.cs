namespace Acme.Workforce.Compliance.PolicyAck.Cycle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual policy ack groups by code.</summary>
public interface IManualPolicyAckGroupStore
{
    ManualPolicyAckGroup? Find(string code);
    void Save(ManualPolicyAckGroup item);
    bool Remove(string code);
    List<ManualPolicyAckGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualPolicyAckGroupStore(Dictionary<string, ManualPolicyAckGroup> items, List<string> order) : IManualPolicyAckGroupStore
{
    public Dictionary<string, ManualPolicyAckGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualPolicyAckGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualPolicyAckGroup item)
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

    public List<ManualPolicyAckGroup> List()
    {
        var outValue = new List<ManualPolicyAckGroup>();
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
public sealed class CachedManualPolicyAckGroupStore(MemoryManualPolicyAckGroupStore inner, ManualPolicyAckGroupCache cache, int hits, int misses) : IManualPolicyAckGroupStore
{
    public MemoryManualPolicyAckGroupStore Inner { get; set; } = inner;
    public ManualPolicyAckGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualPolicyAckGroup? Find(string code)
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

    public void Save(ManualPolicyAckGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualPolicyAckGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual policy ack groups. Empty fields match all.</summary>
public sealed record ManualPolicyAckGroupQuery(string Name, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(ManualPolicyAckGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualPolicyAckGroup> Apply(List<ManualPolicyAckGroup> items)
    {
        var matched = Collections.Filter(items, (ManualPolicyAckGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual policy ack group store.</summary>
public static class ManualPolicyAckGroupStore
{
    public static IManualPolicyAckGroupStore NewMemoryManualPolicyAckGroupStore()
    {
        return new MemoryManualPolicyAckGroupStore(
            new Dictionary<string, ManualPolicyAckGroup>(),
            new List<string>());
    }

    public static IManualPolicyAckGroupStore NewCachedManualPolicyAckGroupStore(MemoryManualPolicyAckGroupStore inner, int ttl, IClock clock)
    {
        return new CachedManualPolicyAckGroupStore(inner, ManualPolicyAckGroupCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<ManualPolicyAckGroup> FindManualPolicyAckGroups(IManualPolicyAckGroupStore store, ManualPolicyAckGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
