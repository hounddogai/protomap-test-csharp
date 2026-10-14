namespace Acme.Workforce.Compliance.Attestation.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default attestation rollups by code.</summary>
public interface IDefaultAttestationRollupStore
{
    DefaultAttestationRollup? Find(string code);
    void Save(DefaultAttestationRollup item);
    bool Remove(string code);
    List<DefaultAttestationRollup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultAttestationRollupStore(Dictionary<string, DefaultAttestationRollup> items, List<string> order) : IDefaultAttestationRollupStore
{
    public Dictionary<string, DefaultAttestationRollup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultAttestationRollup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultAttestationRollup item)
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

    public List<DefaultAttestationRollup> List()
    {
        var outValue = new List<DefaultAttestationRollup>();
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
public sealed class CachedDefaultAttestationRollupStore(MemoryDefaultAttestationRollupStore inner, DefaultAttestationRollupCache cache, int hits, int misses) : IDefaultAttestationRollupStore
{
    public MemoryDefaultAttestationRollupStore Inner { get; set; } = inner;
    public DefaultAttestationRollupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultAttestationRollup? Find(string code)
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

    public void Save(DefaultAttestationRollup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultAttestationRollup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default attestation rollups. Empty fields match all.</summary>
public sealed record DefaultAttestationRollupQuery(string Label, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultAttestationRollup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultAttestationRollup> Apply(List<DefaultAttestationRollup> items)
    {
        var matched = Collections.Filter(items, (DefaultAttestationRollup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default attestation rollup store.</summary>
public static class DefaultAttestationRollupStore
{
    public static IDefaultAttestationRollupStore NewMemoryDefaultAttestationRollupStore()
    {
        return new MemoryDefaultAttestationRollupStore(
            new Dictionary<string, DefaultAttestationRollup>(),
            new List<string>());
    }

    public static IDefaultAttestationRollupStore NewCachedDefaultAttestationRollupStore(MemoryDefaultAttestationRollupStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultAttestationRollupStore(
            inner,
            DefaultAttestationRollupCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<DefaultAttestationRollup> FindDefaultAttestationRollups(IDefaultAttestationRollupStore store, DefaultAttestationRollupQuery query)
    {
        return query.Apply(store.List());
    }
}
