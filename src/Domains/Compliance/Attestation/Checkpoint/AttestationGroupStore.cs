namespace Acme.Workforce.Compliance.Attestation.Checkpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation groups by code.</summary>
public interface IAttestationGroupStore
{
    AttestationGroup? Find(string code);
    void Save(AttestationGroup item);
    bool Remove(string code);
    List<AttestationGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationGroupStore(Dictionary<string, AttestationGroup> items, List<string> order) : IAttestationGroupStore
{
    public Dictionary<string, AttestationGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationGroup item)
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

    public List<AttestationGroup> List()
    {
        var outValue = new List<AttestationGroup>();
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
public sealed class CachedAttestationGroupStore(MemoryAttestationGroupStore inner, AttestationGroupCache cache, int hits, int misses) : IAttestationGroupStore
{
    public MemoryAttestationGroupStore Inner { get; set; } = inner;
    public AttestationGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationGroup? Find(string code)
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

    public void Save(AttestationGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation groups. Empty fields match all.</summary>
public sealed record AttestationGroupQuery(string Name, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationGroup item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationGroup> Apply(List<AttestationGroup> items)
    {
        var matched = Collections.Filter(items, (AttestationGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation group store.</summary>
public static class AttestationGroupStore
{
    public static IAttestationGroupStore NewMemoryAttestationGroupStore()
    {
        return new MemoryAttestationGroupStore(new Dictionary<string, AttestationGroup>(), new List<string>());
    }

    public static IAttestationGroupStore NewCachedAttestationGroupStore(MemoryAttestationGroupStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationGroupStore(inner, AttestationGroupCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationGroup> FindAttestationGroups(IAttestationGroupStore store, AttestationGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
