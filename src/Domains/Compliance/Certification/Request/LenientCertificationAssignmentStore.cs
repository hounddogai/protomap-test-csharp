namespace Acme.Workforce.Compliance.Certification.Request;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient certification assignments by code.</summary>
public interface ILenientCertificationAssignmentStore
{
    LenientCertificationAssignment? Find(string code);
    void Save(LenientCertificationAssignment item);
    bool Remove(string code);
    List<LenientCertificationAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientCertificationAssignmentStore(Dictionary<string, LenientCertificationAssignment> items, List<string> order) : ILenientCertificationAssignmentStore
{
    public Dictionary<string, LenientCertificationAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientCertificationAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientCertificationAssignment item)
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

    public List<LenientCertificationAssignment> List()
    {
        var outValue = new List<LenientCertificationAssignment>();
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
public sealed class CachedLenientCertificationAssignmentStore(MemoryLenientCertificationAssignmentStore inner, LenientCertificationAssignmentCache cache, int hits, int misses) : ILenientCertificationAssignmentStore
{
    public MemoryLenientCertificationAssignmentStore Inner { get; set; } = inner;
    public LenientCertificationAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientCertificationAssignment? Find(string code)
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

    public void Save(LenientCertificationAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientCertificationAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient certification assignments. Empty fields match all.</summary>
public sealed record LenientCertificationAssignmentQuery(string Locale, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LenientCertificationAssignment item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<LenientCertificationAssignment> Apply(List<LenientCertificationAssignment> items)
    {
        var matched = Collections.Filter(items, (LenientCertificationAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient certification assignment store.</summary>
public static class LenientCertificationAssignmentStore
{
    public static ILenientCertificationAssignmentStore NewMemoryLenientCertificationAssignmentStore()
    {
        return new MemoryLenientCertificationAssignmentStore(
            new Dictionary<string, LenientCertificationAssignment>(),
            new List<string>());
    }

    public static ILenientCertificationAssignmentStore NewCachedLenientCertificationAssignmentStore(MemoryLenientCertificationAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLenientCertificationAssignmentStore(
            inner,
            LenientCertificationAssignmentCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LenientCertificationAssignment> FindLenientCertificationAssignments(ILenientCertificationAssignmentStore store, LenientCertificationAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
