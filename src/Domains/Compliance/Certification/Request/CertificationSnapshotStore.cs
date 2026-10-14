namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification snapshots by code.</summary>
public interface ICertificationSnapshotStore
{
    CertificationSnapshot? Find(string code);
    void Save(CertificationSnapshot item);
    bool Remove(string code);
    List<CertificationSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationSnapshotStore(Dictionary<string, CertificationSnapshot> items, List<string> order) : ICertificationSnapshotStore
{
    public Dictionary<string, CertificationSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationSnapshot item)
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

    public List<CertificationSnapshot> List()
    {
        var outValue = new List<CertificationSnapshot>();
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
public sealed class CachedCertificationSnapshotStore(MemoryCertificationSnapshotStore inner, CertificationSnapshotCache cache, int hits, int misses) : ICertificationSnapshotStore
{
    public MemoryCertificationSnapshotStore Inner { get; set; } = inner;
    public CertificationSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationSnapshot? Find(string code)
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

    public void Save(CertificationSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification snapshots. Empty fields match all.</summary>
public sealed record CertificationSnapshotQuery(string Source, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationSnapshot item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Discount < MinDiscount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationSnapshot> Apply(List<CertificationSnapshot> items)
    {
        var matched = Collections.Filter(items, (CertificationSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification snapshot store.</summary>
public static class CertificationSnapshotStore
{
    public static ICertificationSnapshotStore NewMemoryCertificationSnapshotStore()
    {
        return new MemoryCertificationSnapshotStore(
            new Dictionary<string, CertificationSnapshot>(),
            new List<string>());
    }

    public static ICertificationSnapshotStore NewCachedCertificationSnapshotStore(MemoryCertificationSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationSnapshotStore(inner, CertificationSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<CertificationSnapshot> FindCertificationSnapshots(ICertificationSnapshotStore store, CertificationSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
