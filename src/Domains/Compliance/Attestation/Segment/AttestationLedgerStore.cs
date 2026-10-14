namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps attestation ledgers by code.</summary>
public interface IAttestationLedgerStore
{
    AttestationLedger? Find(string code);
    void Save(AttestationLedger item);
    bool Remove(string code);
    List<AttestationLedger> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAttestationLedgerStore(Dictionary<string, AttestationLedger> items, List<string> order) : IAttestationLedgerStore
{
    public Dictionary<string, AttestationLedger> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AttestationLedger? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AttestationLedger item)
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

    public List<AttestationLedger> List()
    {
        var outValue = new List<AttestationLedger>();
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
public sealed class CachedAttestationLedgerStore(MemoryAttestationLedgerStore inner, AttestationLedgerCache cache, int hits, int misses) : IAttestationLedgerStore
{
    public MemoryAttestationLedgerStore Inner { get; set; } = inner;
    public AttestationLedgerCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AttestationLedger? Find(string code)
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

    public void Save(AttestationLedger item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AttestationLedger> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects attestation ledgers. Empty fields match all.</summary>
public sealed record AttestationLedgerQuery(string Channel, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(AttestationLedger item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AttestationLedger> Apply(List<AttestationLedger> items)
    {
        var matched = Collections.Filter(items, (AttestationLedger candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of attestation ledger store.</summary>
public static class AttestationLedgerStore
{
    public static IAttestationLedgerStore NewMemoryAttestationLedgerStore()
    {
        return new MemoryAttestationLedgerStore(new Dictionary<string, AttestationLedger>(), new List<string>());
    }

    public static IAttestationLedgerStore NewCachedAttestationLedgerStore(MemoryAttestationLedgerStore inner, int ttl, IClock clock)
    {
        return new CachedAttestationLedgerStore(inner, AttestationLedgerCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<AttestationLedger> FindAttestationLedgers(IAttestationLedgerStore store, AttestationLedgerQuery query)
    {
        return query.Apply(store.List());
    }
}
