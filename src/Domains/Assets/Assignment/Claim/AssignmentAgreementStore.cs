namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment agreements by code.</summary>
public interface IAssignmentAgreementStore
{
    AssignmentAgreement? Find(string code);
    void Save(AssignmentAgreement item);
    bool Remove(string code);
    List<AssignmentAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentAgreementStore(Dictionary<string, AssignmentAgreement> items, List<string> order) : IAssignmentAgreementStore
{
    public Dictionary<string, AssignmentAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentAgreement item)
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

    public List<AssignmentAgreement> List()
    {
        var outValue = new List<AssignmentAgreement>();
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
public sealed class CachedAssignmentAgreementStore(MemoryAssignmentAgreementStore inner, AssignmentAgreementCache cache, int hits, int misses) : IAssignmentAgreementStore
{
    public MemoryAssignmentAgreementStore Inner { get; set; } = inner;
    public AssignmentAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentAgreement? Find(string code)
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

    public void Save(AssignmentAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment agreements. Empty fields match all.</summary>
public sealed record AssignmentAgreementQuery(string Title, double MinCost, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentAgreement item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Cost < MinCost)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssignmentAgreement> Apply(List<AssignmentAgreement> items)
    {
        var matched = Collections.Filter(items, (AssignmentAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment agreement store.</summary>
public static class AssignmentAgreementStore
{
    public static IAssignmentAgreementStore NewMemoryAssignmentAgreementStore()
    {
        return new MemoryAssignmentAgreementStore(new Dictionary<string, AssignmentAgreement>(), new List<string>());
    }

    public static IAssignmentAgreementStore NewCachedAssignmentAgreementStore(MemoryAssignmentAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentAgreementStore(inner, AssignmentAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<AssignmentAgreement> FindAssignmentAgreements(IAssignmentAgreementStore store, AssignmentAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
