namespace Acme.Workforce.Compliance.Attestation.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation allocations by code.</summary>
public interface IAttestationAllocationStore
{
    AttestationAllocation? Find(string code);
    void Save(AttestationAllocation item);
    bool Remove(string code);
    List<AttestationAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationAllocationStore(Dictionary<string, AttestationAllocation> items, List<string> order) : IAttestationAllocationStore
{
    public Dictionary<string, AttestationAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationAllocation item)
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

    public List<AttestationAllocation> List()
    {
        var outValue = new List<AttestationAllocation>();
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
public sealed class CachedAttestationAllocationStore(MemoryAttestationAllocationStore inner, AttestationAllocationCache cache, int hits, int misses) : IAttestationAllocationStore
{
    public MemoryAttestationAllocationStore Inner { get; set; } = inner;
    public AttestationAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationAllocation? Find(string code)
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

    public void Save(AttestationAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation allocations. Empty fields match all.</summary>
public sealed record AttestationAllocationQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationAllocation> Apply(List<AttestationAllocation> items)
    {
        var matched = Collections.Filter(items, (AttestationAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation allocation store.</summary>
public static class AttestationAllocationStore
{
    public static IAttestationAllocationStore NewMemoryAttestationAllocationStore()
    {
        return new MemoryAttestationAllocationStore(
            new Dictionary<string, AttestationAllocation>(),
            new List<string>());
    }

    public static IAttestationAllocationStore NewCachedAttestationAllocationStore(MemoryAttestationAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationAllocationStore(inner, AttestationAllocationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationAllocation> FindAttestationAllocations(IAttestationAllocationStore store, AttestationAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
