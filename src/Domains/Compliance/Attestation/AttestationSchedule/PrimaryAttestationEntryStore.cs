namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary attestation entries by code.</summary>
public interface IPrimaryAttestationEntryStore
{
    PrimaryAttestationEntry? Find(string code);
    void Save(PrimaryAttestationEntry item);
    bool Remove(string code);
    List<PrimaryAttestationEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryAttestationEntryStore(Dictionary<string, PrimaryAttestationEntry> items, List<string> order) : IPrimaryAttestationEntryStore
{
    public Dictionary<string, PrimaryAttestationEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryAttestationEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryAttestationEntry item)
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

    public List<PrimaryAttestationEntry> List()
    {
        var outValue = new List<PrimaryAttestationEntry>();
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
public sealed class CachedPrimaryAttestationEntryStore(MemoryPrimaryAttestationEntryStore inner, PrimaryAttestationEntryCache cache, int hits, int misses) : IPrimaryAttestationEntryStore
{
    public MemoryPrimaryAttestationEntryStore Inner { get; set; } = inner;
    public PrimaryAttestationEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryAttestationEntry? Find(string code)
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

    public void Save(PrimaryAttestationEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryAttestationEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary attestation entries. Empty fields match all.</summary>
public sealed record PrimaryAttestationEntryQuery(string Category, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryAttestationEntry item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryAttestationEntry> Apply(List<PrimaryAttestationEntry> items)
    {
        var matched = Collections.Filter(items, (PrimaryAttestationEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary attestation entry store.</summary>
public static class PrimaryAttestationEntryStore
{
    public static IPrimaryAttestationEntryStore NewMemoryPrimaryAttestationEntryStore()
    {
        return new MemoryPrimaryAttestationEntryStore(
            new Dictionary<string, PrimaryAttestationEntry>(),
            new List<string>());
    }

    public static IPrimaryAttestationEntryStore NewCachedPrimaryAttestationEntryStore(MemoryPrimaryAttestationEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryAttestationEntryStore(
            inner,
            PrimaryAttestationEntryCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryAttestationEntry> FindPrimaryAttestationEntries(IPrimaryAttestationEntryStore store, PrimaryAttestationEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
