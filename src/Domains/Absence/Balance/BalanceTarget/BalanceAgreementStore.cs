namespace Acme.Workforce.Absence.Balance.BalanceTarget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps balance agreements by code.</summary>
public interface IBalanceAgreementStore
{
    BalanceAgreement? Find(string code);
    void Save(BalanceAgreement item);
    bool Remove(string code);
    List<BalanceAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBalanceAgreementStore(Dictionary<string, BalanceAgreement> items, List<string> order) : IBalanceAgreementStore
{
    public Dictionary<string, BalanceAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BalanceAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BalanceAgreement item)
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

    public List<BalanceAgreement> List()
    {
        var outValue = new List<BalanceAgreement>();
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
public sealed class CachedBalanceAgreementStore(MemoryBalanceAgreementStore inner, BalanceAgreementCache cache, int hits, int misses) : IBalanceAgreementStore
{
    public MemoryBalanceAgreementStore Inner { get; set; } = inner;
    public BalanceAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BalanceAgreement? Find(string code)
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

    public void Save(BalanceAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BalanceAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects balance agreements. Empty fields match all.</summary>
public sealed record BalanceAgreementQuery(string Label, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(BalanceAgreement item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<BalanceAgreement> Apply(List<BalanceAgreement> items)
    {
        var matched = Collections.Filter(items, (BalanceAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of balance agreement store.</summary>
public static class BalanceAgreementStore
{
    public static IBalanceAgreementStore NewMemoryBalanceAgreementStore()
    {
        return new MemoryBalanceAgreementStore(new Dictionary<string, BalanceAgreement>(), new List<string>());
    }

    public static IBalanceAgreementStore NewCachedBalanceAgreementStore(MemoryBalanceAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedBalanceAgreementStore(inner, BalanceAgreementCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<BalanceAgreement> FindBalanceAgreements(IBalanceAgreementStore store, BalanceAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
