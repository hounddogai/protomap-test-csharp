namespace Acme.Workforce.Workplace.Visitor.Policy;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps visitor agreements by code.</summary>
public interface IVisitorAgreementStore
{
    VisitorAgreement? Find(string code);
    void Save(VisitorAgreement item);
    bool Remove(string code);
    List<VisitorAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryVisitorAgreementStore(Dictionary<string, VisitorAgreement> items, List<string> order) : IVisitorAgreementStore
{
    public Dictionary<string, VisitorAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public VisitorAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(VisitorAgreement item)
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

    public List<VisitorAgreement> List()
    {
        var outValue = new List<VisitorAgreement>();
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
public sealed class CachedVisitorAgreementStore(MemoryVisitorAgreementStore inner, VisitorAgreementCache cache, int hits, int misses) : IVisitorAgreementStore
{
    public MemoryVisitorAgreementStore Inner { get; set; } = inner;
    public VisitorAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public VisitorAgreement? Find(string code)
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

    public void Save(VisitorAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<VisitorAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects visitor agreements. Empty fields match all.</summary>
public sealed record VisitorAgreementQuery(string Name, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(VisitorAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<VisitorAgreement> Apply(List<VisitorAgreement> items)
    {
        var matched = Collections.Filter(items, (VisitorAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of visitor agreement store.</summary>
public static class VisitorAgreementStore
{
    public static IVisitorAgreementStore NewMemoryVisitorAgreementStore()
    {
        return new MemoryVisitorAgreementStore(new Dictionary<string, VisitorAgreement>(), new List<string>());
    }

    public static IVisitorAgreementStore NewCachedVisitorAgreementStore(MemoryVisitorAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedVisitorAgreementStore(inner, VisitorAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<VisitorAgreement> FindVisitorAgreements(IVisitorAgreementStore store, VisitorAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
