namespace Acme.Workforce.Compliance.Attestation.AttestationDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pending attestation thresholds by code.</summary>
public interface IPendingAttestationThresholdStore
{
    PendingAttestationThreshold? Find(string code);
    void Save(PendingAttestationThreshold item);
    bool Remove(string code);
    List<PendingAttestationThreshold> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPendingAttestationThresholdStore(Dictionary<string, PendingAttestationThreshold> items, List<string> order) : IPendingAttestationThresholdStore
{
    public Dictionary<string, PendingAttestationThreshold> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PendingAttestationThreshold? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PendingAttestationThreshold item)
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

    public List<PendingAttestationThreshold> List()
    {
        var outValue = new List<PendingAttestationThreshold>();
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
public sealed class CachedPendingAttestationThresholdStore(MemoryPendingAttestationThresholdStore inner, PendingAttestationThresholdCache cache, int hits, int misses) : IPendingAttestationThresholdStore
{
    public MemoryPendingAttestationThresholdStore Inner { get; set; } = inner;
    public PendingAttestationThresholdCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PendingAttestationThreshold? Find(string code)
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

    public void Save(PendingAttestationThreshold item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PendingAttestationThreshold> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pending attestation thresholds. Empty fields match all.</summary>
public sealed record PendingAttestationThresholdQuery(string Department, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PendingAttestationThreshold item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PendingAttestationThreshold> Apply(List<PendingAttestationThreshold> items)
    {
        var matched = Collections.Filter(items, (PendingAttestationThreshold candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pending attestation threshold store.</summary>
public static class PendingAttestationThresholdStore
{
    public static IPendingAttestationThresholdStore NewMemoryPendingAttestationThresholdStore()
    {
        return new MemoryPendingAttestationThresholdStore(
            new Dictionary<string, PendingAttestationThreshold>(),
            new List<string>());
    }

    public static IPendingAttestationThresholdStore NewCachedPendingAttestationThresholdStore(MemoryPendingAttestationThresholdStore inner, int ttl, IClock clock)
    {
        return new CachedPendingAttestationThresholdStore(
            inner,
            PendingAttestationThresholdCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<PendingAttestationThreshold> FindPendingAttestationThresholds(IPendingAttestationThresholdStore store, PendingAttestationThresholdQuery query)
    {
        return query.Apply(store.List());
    }
}
