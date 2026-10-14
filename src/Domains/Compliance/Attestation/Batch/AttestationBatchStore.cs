namespace Acme.Workforce.Compliance.Attestation.Batch;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation batches by code.</summary>
public interface IAttestationBatchStore
{
    AttestationBatch? Find(string code);
    void Save(AttestationBatch item);
    bool Remove(string code);
    List<AttestationBatch> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationBatchStore(Dictionary<string, AttestationBatch> items, List<string> order) : IAttestationBatchStore
{
    public Dictionary<string, AttestationBatch> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationBatch? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationBatch item)
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

    public List<AttestationBatch> List()
    {
        var outValue = new List<AttestationBatch>();
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
public sealed class CachedAttestationBatchStore(MemoryAttestationBatchStore inner, AttestationBatchCache cache, int hits, int misses) : IAttestationBatchStore
{
    public MemoryAttestationBatchStore Inner { get; set; } = inner;
    public AttestationBatchCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationBatch? Find(string code)
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

    public void Save(AttestationBatch item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationBatch> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation batches. Empty fields match all.</summary>
public sealed record AttestationBatchQuery(string Locale, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationBatch item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<AttestationBatch> Apply(List<AttestationBatch> items)
    {
        var matched = Collections.Filter(items, (AttestationBatch candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation batch store.</summary>
public static class AttestationBatchStore
{
    public static IAttestationBatchStore NewMemoryAttestationBatchStore()
    {
        return new MemoryAttestationBatchStore(new Dictionary<string, AttestationBatch>(), new List<string>());
    }

    public static IAttestationBatchStore NewCachedAttestationBatchStore(MemoryAttestationBatchStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationBatchStore(inner, AttestationBatchCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AttestationBatch> FindAttestationBatches(IAttestationBatchStore store, AttestationBatchQuery query)
    {
        return query.Apply(store.List());
    }
}
