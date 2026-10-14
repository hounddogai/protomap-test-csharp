namespace Acme.Workforce.Compliance.Attestation.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy attestation entries by code.</summary>
public interface ILegacyAttestationEntryStore
{
    LegacyAttestationEntry? Find(string code);
    void Save(LegacyAttestationEntry item);
    bool Remove(string code);
    List<LegacyAttestationEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyAttestationEntryStore(Dictionary<string, LegacyAttestationEntry> items, List<string> order) : ILegacyAttestationEntryStore
{
    public Dictionary<string, LegacyAttestationEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyAttestationEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyAttestationEntry item)
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

    public List<LegacyAttestationEntry> List()
    {
        var outValue = new List<LegacyAttestationEntry>();
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
public sealed class CachedLegacyAttestationEntryStore(MemoryLegacyAttestationEntryStore inner, LegacyAttestationEntryCache cache, int hits, int misses) : ILegacyAttestationEntryStore
{
    public MemoryLegacyAttestationEntryStore Inner { get; set; } = inner;
    public LegacyAttestationEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyAttestationEntry? Find(string code)
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

    public void Save(LegacyAttestationEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyAttestationEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy attestation entries. Empty fields match all.</summary>
public sealed record LegacyAttestationEntryQuery(string Region, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyAttestationEntry item)
    {
        if (Region != "" && Text.Normalize(item.Region) != Text.Normalize(Region))
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

    public List<LegacyAttestationEntry> Apply(List<LegacyAttestationEntry> items)
    {
        var matched = Collections.Filter(items, (LegacyAttestationEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy attestation entry store.</summary>
public static class LegacyAttestationEntryStore
{
    public static ILegacyAttestationEntryStore NewMemoryLegacyAttestationEntryStore()
    {
        return new MemoryLegacyAttestationEntryStore(
            new Dictionary<string, LegacyAttestationEntry>(),
            new List<string>());
    }

    public static ILegacyAttestationEntryStore NewCachedLegacyAttestationEntryStore(MemoryLegacyAttestationEntryStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyAttestationEntryStore(
            inner,
            LegacyAttestationEntryCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LegacyAttestationEntry> FindLegacyAttestationEntries(ILegacyAttestationEntryStore store, LegacyAttestationEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
