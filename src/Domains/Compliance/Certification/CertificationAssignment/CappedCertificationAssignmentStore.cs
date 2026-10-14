namespace Acme.Workforce.Compliance.Certification.CertificationAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped certification assignments by code.</summary>
public interface ICappedCertificationAssignmentStore
{
    CappedCertificationAssignment? Find(string code);
    void Save(CappedCertificationAssignment item);
    bool Remove(string code);
    List<CappedCertificationAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedCertificationAssignmentStore(Dictionary<string, CappedCertificationAssignment> items, List<string> order) : ICappedCertificationAssignmentStore
{
    public Dictionary<string, CappedCertificationAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedCertificationAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedCertificationAssignment item)
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

    public List<CappedCertificationAssignment> List()
    {
        var outValue = new List<CappedCertificationAssignment>();
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
public sealed class CachedCappedCertificationAssignmentStore(MemoryCappedCertificationAssignmentStore inner, CappedCertificationAssignmentCache cache, int hits, int misses) : ICappedCertificationAssignmentStore
{
    public MemoryCappedCertificationAssignmentStore Inner { get; set; } = inner;
    public CappedCertificationAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedCertificationAssignment? Find(string code)
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

    public void Save(CappedCertificationAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedCertificationAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped certification assignments. Empty fields match all.</summary>
public sealed record CappedCertificationAssignmentQuery(string Locale, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(CappedCertificationAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<CappedCertificationAssignment> Apply(List<CappedCertificationAssignment> items)
    {
        var matched = Collections.Filter(items, (CappedCertificationAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped certification assignment store.</summary>
public static class CappedCertificationAssignmentStore
{
    public static ICappedCertificationAssignmentStore NewMemoryCappedCertificationAssignmentStore()
    {
        return new MemoryCappedCertificationAssignmentStore(
            new Dictionary<string, CappedCertificationAssignment>(),
            new List<string>());
    }

    public static ICappedCertificationAssignmentStore NewCachedCappedCertificationAssignmentStore(MemoryCappedCertificationAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedCappedCertificationAssignmentStore(
            inner,
            CappedCertificationAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<CappedCertificationAssignment> FindCappedCertificationAssignments(ICappedCertificationAssignmentStore store, CappedCertificationAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
