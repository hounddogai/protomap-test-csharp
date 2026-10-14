namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps certification slots by code.</summary>
public interface ICertificationSlotStore
{
    CertificationSlot? Find(string code);
    void Save(CertificationSlot item);
    bool Remove(string code);
    List<CertificationSlot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCertificationSlotStore(Dictionary<string, CertificationSlot> items, List<string> order) : ICertificationSlotStore
{
    public Dictionary<string, CertificationSlot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CertificationSlot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CertificationSlot item)
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

    public List<CertificationSlot> List()
    {
        var outValue = new List<CertificationSlot>();
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
public sealed class CachedCertificationSlotStore(MemoryCertificationSlotStore inner, CertificationSlotCache cache, int hits, int misses) : ICertificationSlotStore
{
    public MemoryCertificationSlotStore Inner { get; set; } = inner;
    public CertificationSlotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CertificationSlot? Find(string code)
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

    public void Save(CertificationSlot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CertificationSlot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects certification slots. Empty fields match all.</summary>
public sealed record CertificationSlotQuery(string Title, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(CertificationSlot item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<CertificationSlot> Apply(List<CertificationSlot> items)
    {
        var matched = Collections.Filter(items, (CertificationSlot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of certification slot store.</summary>
public static class CertificationSlotStore
{
    public static ICertificationSlotStore NewMemoryCertificationSlotStore()
    {
        return new MemoryCertificationSlotStore(new Dictionary<string, CertificationSlot>(), new List<string>());
    }

    public static ICertificationSlotStore NewCachedCertificationSlotStore(MemoryCertificationSlotStore inner, int ttl, IClock clock)
    {
        return new CachedCertificationSlotStore(inner, CertificationSlotCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CertificationSlot> FindCertificationSlots(ICertificationSlotStore store, CertificationSlotQuery query)
    {
        return query.Apply(store.List());
    }
}
