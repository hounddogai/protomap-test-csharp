namespace Acme.Workforce.Compliance.Attestation.Estimate;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation entries by code.</summary>
public interface IAttestationEntryStore
{
    AttestationEntry? Find(string code);
    void Save(AttestationEntry item);
    bool Remove(string code);
    List<AttestationEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationEntryStore(Dictionary<string, AttestationEntry> items, List<string> order) : IAttestationEntryStore
{
    public Dictionary<string, AttestationEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationEntry item)
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

    public List<AttestationEntry> List()
    {
        var outValue = new List<AttestationEntry>();
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
public sealed class CachedAttestationEntryStore(MemoryAttestationEntryStore inner, AttestationEntryCache cache, int hits, int misses) : IAttestationEntryStore
{
    public MemoryAttestationEntryStore Inner { get; set; } = inner;
    public AttestationEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationEntry? Find(string code)
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

    public void Save(AttestationEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation entries. Empty fields match all.</summary>
public sealed record AttestationEntryQuery(string Source, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationEntry item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
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

    public List<AttestationEntry> Apply(List<AttestationEntry> items)
    {
        var matched = Collections.Filter(items, (AttestationEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation entry store.</summary>
public static class AttestationEntryStore
{
    public static IAttestationEntryStore NewMemoryAttestationEntryStore()
    {
        return new MemoryAttestationEntryStore(new Dictionary<string, AttestationEntry>(), new List<string>());
    }

    public static IAttestationEntryStore NewCachedAttestationEntryStore(MemoryAttestationEntryStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationEntryStore(inner, AttestationEntryCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<AttestationEntry> FindAttestationEntries(IAttestationEntryStore store, AttestationEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
