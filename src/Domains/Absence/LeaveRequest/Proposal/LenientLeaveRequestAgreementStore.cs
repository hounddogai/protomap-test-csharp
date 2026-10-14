namespace Acme.Workforce.Absence.LeaveRequest.Proposal;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps lenient leave request agreements by code.</summary>
public interface ILenientLeaveRequestAgreementStore
{
    LenientLeaveRequestAgreement? Find(string code);
    void Save(LenientLeaveRequestAgreement item);
    bool Remove(string code);
    List<LenientLeaveRequestAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLenientLeaveRequestAgreementStore(Dictionary<string, LenientLeaveRequestAgreement> items, List<string> order) : ILenientLeaveRequestAgreementStore
{
    public Dictionary<string, LenientLeaveRequestAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LenientLeaveRequestAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LenientLeaveRequestAgreement item)
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

    public List<LenientLeaveRequestAgreement> List()
    {
        var outValue = new List<LenientLeaveRequestAgreement>();
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
public sealed class CachedLenientLeaveRequestAgreementStore(MemoryLenientLeaveRequestAgreementStore inner, LenientLeaveRequestAgreementCache cache, int hits, int misses) : ILenientLeaveRequestAgreementStore
{
    public MemoryLenientLeaveRequestAgreementStore Inner { get; set; } = inner;
    public LenientLeaveRequestAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LenientLeaveRequestAgreement? Find(string code)
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

    public void Save(LenientLeaveRequestAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LenientLeaveRequestAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects lenient leave request agreements. Empty fields match all.</summary>
public sealed record LenientLeaveRequestAgreementQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(LenientLeaveRequestAgreement item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LenientLeaveRequestAgreement> Apply(List<LenientLeaveRequestAgreement> items)
    {
        var matched = Collections.Filter(items, (LenientLeaveRequestAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of lenient leave request agreement store.</summary>
public static class LenientLeaveRequestAgreementStore
{
    public static ILenientLeaveRequestAgreementStore NewMemoryLenientLeaveRequestAgreementStore()
    {
        return new MemoryLenientLeaveRequestAgreementStore(
            new Dictionary<string, LenientLeaveRequestAgreement>(),
            new List<string>());
    }

    public static ILenientLeaveRequestAgreementStore NewCachedLenientLeaveRequestAgreementStore(MemoryLenientLeaveRequestAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedLenientLeaveRequestAgreementStore(
            inner,
            LenientLeaveRequestAgreementCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<LenientLeaveRequestAgreement> FindLenientLeaveRequestAgreements(ILenientLeaveRequestAgreementStore store, LenientLeaveRequestAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
