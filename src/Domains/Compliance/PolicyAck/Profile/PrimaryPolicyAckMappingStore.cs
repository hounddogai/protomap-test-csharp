namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary policy ack mappings by code.</summary>
public interface IPrimaryPolicyAckMappingStore
{
    PrimaryPolicyAckMapping? Find(string code);
    void Save(PrimaryPolicyAckMapping item);
    bool Remove(string code);
    List<PrimaryPolicyAckMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryPolicyAckMappingStore(Dictionary<string, PrimaryPolicyAckMapping> items, List<string> order) : IPrimaryPolicyAckMappingStore
{
    public Dictionary<string, PrimaryPolicyAckMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryPolicyAckMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryPolicyAckMapping item)
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

    public List<PrimaryPolicyAckMapping> List()
    {
        var outValue = new List<PrimaryPolicyAckMapping>();
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
public sealed class CachedPrimaryPolicyAckMappingStore(MemoryPrimaryPolicyAckMappingStore inner, PrimaryPolicyAckMappingCache cache, int hits, int misses) : IPrimaryPolicyAckMappingStore
{
    public MemoryPrimaryPolicyAckMappingStore Inner { get; set; } = inner;
    public PrimaryPolicyAckMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryPolicyAckMapping? Find(string code)
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

    public void Save(PrimaryPolicyAckMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryPolicyAckMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary policy ack mappings. Empty fields match all.</summary>
public sealed record PrimaryPolicyAckMappingQuery(string Locale, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryPolicyAckMapping item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryPolicyAckMapping> Apply(List<PrimaryPolicyAckMapping> items)
    {
        var matched = Collections.Filter(items, (PrimaryPolicyAckMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary policy ack mapping store.</summary>
public static class PrimaryPolicyAckMappingStore
{
    public static IPrimaryPolicyAckMappingStore NewMemoryPrimaryPolicyAckMappingStore()
    {
        return new MemoryPrimaryPolicyAckMappingStore(
            new Dictionary<string, PrimaryPolicyAckMapping>(),
            new List<string>());
    }

    public static IPrimaryPolicyAckMappingStore NewCachedPrimaryPolicyAckMappingStore(MemoryPrimaryPolicyAckMappingStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryPolicyAckMappingStore(
            inner,
            PrimaryPolicyAckMappingCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryPolicyAckMapping> FindPrimaryPolicyAckMappings(IPrimaryPolicyAckMappingStore store, PrimaryPolicyAckMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
