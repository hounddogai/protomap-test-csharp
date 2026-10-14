namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps equity grant agreements by code.</summary>
public interface IEquityGrantAgreementStore
{
    EquityGrantAgreement? Find(string code);
    void Save(EquityGrantAgreement item);
    bool Remove(string code);
    List<EquityGrantAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryEquityGrantAgreementStore(Dictionary<string, EquityGrantAgreement> items, List<string> order) : IEquityGrantAgreementStore
{
    public Dictionary<string, EquityGrantAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public EquityGrantAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(EquityGrantAgreement item)
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

    public List<EquityGrantAgreement> List()
    {
        var outValue = new List<EquityGrantAgreement>();
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
public sealed class CachedEquityGrantAgreementStore(MemoryEquityGrantAgreementStore inner, EquityGrantAgreementCache cache, int hits, int misses) : IEquityGrantAgreementStore
{
    public MemoryEquityGrantAgreementStore Inner { get; set; } = inner;
    public EquityGrantAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public EquityGrantAgreement? Find(string code)
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

    public void Save(EquityGrantAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<EquityGrantAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects equity grant agreements. Empty fields match all.</summary>
public sealed record EquityGrantAgreementQuery(string Segment, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(EquityGrantAgreement item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.LeaveDays < MinLeaveDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<EquityGrantAgreement> Apply(List<EquityGrantAgreement> items)
    {
        var matched = Collections.Filter(items, (EquityGrantAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of equity grant agreement store.</summary>
public static class EquityGrantAgreementStore
{
    public static IEquityGrantAgreementStore NewMemoryEquityGrantAgreementStore()
    {
        return new MemoryEquityGrantAgreementStore(
            new Dictionary<string, EquityGrantAgreement>(),
            new List<string>());
    }

    public static IEquityGrantAgreementStore NewCachedEquityGrantAgreementStore(MemoryEquityGrantAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedEquityGrantAgreementStore(inner, EquityGrantAgreementCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<EquityGrantAgreement> FindEquityGrantAgreements(IEquityGrantAgreementStore store, EquityGrantAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
