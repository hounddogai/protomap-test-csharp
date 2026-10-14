namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default attestation allocations by code.</summary>
public interface IDefaultAttestationAllocationStore
{
    DefaultAttestationAllocation? Find(string code);
    void Save(DefaultAttestationAllocation item);
    bool Remove(string code);
    List<DefaultAttestationAllocation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAttestationAllocationStore(Dictionary<string, DefaultAttestationAllocation> items, List<string> order) : IDefaultAttestationAllocationStore
{
    public Dictionary<string, DefaultAttestationAllocation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAttestationAllocation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAttestationAllocation item)
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

    public List<DefaultAttestationAllocation> List()
    {
        var outValue = new List<DefaultAttestationAllocation>();
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
public sealed class CachedDefaultAttestationAllocationStore(MemoryDefaultAttestationAllocationStore inner, DefaultAttestationAllocationCache cache, int hits, int misses) : IDefaultAttestationAllocationStore
{
    public MemoryDefaultAttestationAllocationStore Inner { get; set; } = inner;
    public DefaultAttestationAllocationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAttestationAllocation? Find(string code)
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

    public void Save(DefaultAttestationAllocation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAttestationAllocation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default attestation allocations. Empty fields match all.</summary>
public sealed record DefaultAttestationAllocationQuery(string Title, double MinCeiling, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAttestationAllocation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<DefaultAttestationAllocation> Apply(List<DefaultAttestationAllocation> items)
    {
        var matched = Collections.Filter(items, (DefaultAttestationAllocation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default attestation allocation store.</summary>
public static class DefaultAttestationAllocationStore
{
    public static IDefaultAttestationAllocationStore NewMemoryDefaultAttestationAllocationStore()
    {
        return new MemoryDefaultAttestationAllocationStore(
            new Dictionary<string, DefaultAttestationAllocation>(),
            new List<string>());
    }

    public static IDefaultAttestationAllocationStore NewCachedDefaultAttestationAllocationStore(MemoryDefaultAttestationAllocationStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAttestationAllocationStore(
            inner,
            DefaultAttestationAllocationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAttestationAllocation> FindDefaultAttestationAllocations(IDefaultAttestationAllocationStore store, DefaultAttestationAllocationQuery query)
    {
        return query.Apply(store.List());
    }
}
