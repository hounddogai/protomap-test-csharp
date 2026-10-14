namespace Acme.Workforce.Payrollops.TaxForm.FormAssignment;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy tax form assignments by code.</summary>
public interface ILegacyTaxFormAssignmentStore
{
    LegacyTaxFormAssignment? Find(string code);
    void Save(LegacyTaxFormAssignment item);
    bool Remove(string code);
    List<LegacyTaxFormAssignment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyTaxFormAssignmentStore(Dictionary<string, LegacyTaxFormAssignment> items, List<string> order) : ILegacyTaxFormAssignmentStore
{
    public Dictionary<string, LegacyTaxFormAssignment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyTaxFormAssignment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyTaxFormAssignment item)
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

    public List<LegacyTaxFormAssignment> List()
    {
        var outValue = new List<LegacyTaxFormAssignment>();
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
public sealed class CachedLegacyTaxFormAssignmentStore(MemoryLegacyTaxFormAssignmentStore inner, LegacyTaxFormAssignmentCache cache, int hits, int misses) : ILegacyTaxFormAssignmentStore
{
    public MemoryLegacyTaxFormAssignmentStore Inner { get; set; } = inner;
    public LegacyTaxFormAssignmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyTaxFormAssignment? Find(string code)
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

    public void Save(LegacyTaxFormAssignment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyTaxFormAssignment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy tax form assignments. Empty fields match all.</summary>
public sealed record LegacyTaxFormAssignmentQuery(string Category, int MinExpiresAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyTaxFormAssignment item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.ExpiresAt < MinExpiresAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LegacyTaxFormAssignment> Apply(List<LegacyTaxFormAssignment> items)
    {
        var matched = Collections.Filter(items, (LegacyTaxFormAssignment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy tax form assignment store.</summary>
public static class LegacyTaxFormAssignmentStore
{
    public static ILegacyTaxFormAssignmentStore NewMemoryLegacyTaxFormAssignmentStore()
    {
        return new MemoryLegacyTaxFormAssignmentStore(
            new Dictionary<string, LegacyTaxFormAssignment>(),
            new List<string>());
    }

    public static ILegacyTaxFormAssignmentStore NewCachedLegacyTaxFormAssignmentStore(MemoryLegacyTaxFormAssignmentStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyTaxFormAssignmentStore(
            inner,
            LegacyTaxFormAssignmentCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<LegacyTaxFormAssignment> FindLegacyTaxFormAssignments(ILegacyTaxFormAssignmentStore store, LegacyTaxFormAssignmentQuery query)
    {
        return query.Apply(store.List());
    }
}
