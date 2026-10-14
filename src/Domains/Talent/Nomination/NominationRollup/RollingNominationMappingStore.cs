namespace Acme.Workforce.Talent.Nomination.NominationRollup;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling nomination mappings by code.</summary>
public interface IRollingNominationMappingStore
{
    RollingNominationMapping? Find(string code);
    void Save(RollingNominationMapping item);
    bool Remove(string code);
    List<RollingNominationMapping> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingNominationMappingStore(Dictionary<string, RollingNominationMapping> items, List<string> order) : IRollingNominationMappingStore
{
    public Dictionary<string, RollingNominationMapping> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingNominationMapping? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingNominationMapping item)
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

    public List<RollingNominationMapping> List()
    {
        var outValue = new List<RollingNominationMapping>();
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
public sealed class CachedRollingNominationMappingStore(MemoryRollingNominationMappingStore inner, RollingNominationMappingCache cache, int hits, int misses) : IRollingNominationMappingStore
{
    public MemoryRollingNominationMappingStore Inner { get; set; } = inner;
    public RollingNominationMappingCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingNominationMapping? Find(string code)
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

    public void Save(RollingNominationMapping item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingNominationMapping> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling nomination mappings. Empty fields match all.</summary>
public sealed record RollingNominationMappingQuery(string Label, int MinSequence, List<string> Statuses, int Limit)
{
    public bool Matches(RollingNominationMapping item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Sequence < MinSequence)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingNominationMapping> Apply(List<RollingNominationMapping> items)
    {
        var matched = Collections.Filter(items, (RollingNominationMapping candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling nomination mapping store.</summary>
public static class RollingNominationMappingStore
{
    public static IRollingNominationMappingStore NewMemoryRollingNominationMappingStore()
    {
        return new MemoryRollingNominationMappingStore(
            new Dictionary<string, RollingNominationMapping>(),
            new List<string>());
    }

    public static IRollingNominationMappingStore NewCachedRollingNominationMappingStore(MemoryRollingNominationMappingStore inner, int ttl, IClock clock)
    {
        return new CachedRollingNominationMappingStore(
            inner,
            RollingNominationMappingCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RollingNominationMapping> FindRollingNominationMappings(IRollingNominationMappingStore store, RollingNominationMappingQuery query)
    {
        return query.Apply(store.List());
    }
}
