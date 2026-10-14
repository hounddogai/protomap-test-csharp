namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending policy ack groups by code.</summary>
public interface IPendingPolicyAckGroupStore
{
    PendingPolicyAckGroup? Find(string code);
    void Save(PendingPolicyAckGroup item);
    bool Remove(string code);
    List<PendingPolicyAckGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingPolicyAckGroupStore(Dictionary<string, PendingPolicyAckGroup> items, List<string> order) : IPendingPolicyAckGroupStore
{
    public Dictionary<string, PendingPolicyAckGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingPolicyAckGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingPolicyAckGroup item)
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

    public List<PendingPolicyAckGroup> List()
    {
        var outValue = new List<PendingPolicyAckGroup>();
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
public sealed class CachedPendingPolicyAckGroupStore(MemoryPendingPolicyAckGroupStore inner, PendingPolicyAckGroupCache cache, int hits, int misses) : IPendingPolicyAckGroupStore
{
    public MemoryPendingPolicyAckGroupStore Inner { get; set; } = inner;
    public PendingPolicyAckGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingPolicyAckGroup? Find(string code)
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

    public void Save(PendingPolicyAckGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingPolicyAckGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending policy ack groups. Empty fields match all.</summary>
public sealed record PendingPolicyAckGroupQuery(string Title, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(PendingPolicyAckGroup item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingPolicyAckGroup> Apply(List<PendingPolicyAckGroup> items)
    {
        var matched = Collections.Filter(items, (PendingPolicyAckGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending policy ack group store.</summary>
public static class PendingPolicyAckGroupStore
{
    public static IPendingPolicyAckGroupStore NewMemoryPendingPolicyAckGroupStore()
    {
        return new MemoryPendingPolicyAckGroupStore(
            new Dictionary<string, PendingPolicyAckGroup>(),
            new List<string>());
    }

    public static IPendingPolicyAckGroupStore NewCachedPendingPolicyAckGroupStore(MemoryPendingPolicyAckGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPendingPolicyAckGroupStore(inner, PendingPolicyAckGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<PendingPolicyAckGroup> FindPendingPolicyAckGroups(IPendingPolicyAckGroupStore store, PendingPolicyAckGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
