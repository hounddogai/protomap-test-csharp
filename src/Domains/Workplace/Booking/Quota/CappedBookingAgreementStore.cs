namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps capped booking agreements by code.</summary>
public interface ICappedBookingAgreementStore
{
    CappedBookingAgreement? Find(string code);
    void Save(CappedBookingAgreement item);
    bool Remove(string code);
    List<CappedBookingAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCappedBookingAgreementStore(Dictionary<string, CappedBookingAgreement> items, List<string> order) : ICappedBookingAgreementStore
{
    public Dictionary<string, CappedBookingAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CappedBookingAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CappedBookingAgreement item)
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

    public List<CappedBookingAgreement> List()
    {
        var outValue = new List<CappedBookingAgreement>();
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
public sealed class CachedCappedBookingAgreementStore(MemoryCappedBookingAgreementStore inner, CappedBookingAgreementCache cache, int hits, int misses) : ICappedBookingAgreementStore
{
    public MemoryCappedBookingAgreementStore Inner { get; set; } = inner;
    public CappedBookingAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CappedBookingAgreement? Find(string code)
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

    public void Save(CappedBookingAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CappedBookingAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects capped booking agreements. Empty fields match all.</summary>
public sealed record CappedBookingAgreementQuery(string Name, int MinLeaveDays, List<string> Statuses, int Limit)
{
    public bool Matches(CappedBookingAgreement item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
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

    public List<CappedBookingAgreement> Apply(List<CappedBookingAgreement> items)
    {
        var matched = Collections.Filter(items, (CappedBookingAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of capped booking agreement store.</summary>
public static class CappedBookingAgreementStore
{
    public static ICappedBookingAgreementStore NewMemoryCappedBookingAgreementStore()
    {
        return new MemoryCappedBookingAgreementStore(
            new Dictionary<string, CappedBookingAgreement>(),
            new List<string>());
    }

    public static ICappedBookingAgreementStore NewCachedCappedBookingAgreementStore(MemoryCappedBookingAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedCappedBookingAgreementStore(
            inner,
            CappedBookingAgreementCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<CappedBookingAgreement> FindCappedBookingAgreements(ICappedBookingAgreementStore store, CappedBookingAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
