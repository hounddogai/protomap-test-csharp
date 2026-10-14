namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run profiles by code.</summary>
public interface IPayRunProfileStore
{
    PayRunProfile? Find(string code);
    void Save(PayRunProfile item);
    bool Remove(string code);
    List<PayRunProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunProfileStore(Dictionary<string, PayRunProfile> items, List<string> order) : IPayRunProfileStore
{
    public Dictionary<string, PayRunProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunProfile item)
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

    public List<PayRunProfile> List()
    {
        var outValue = new List<PayRunProfile>();
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
public sealed class CachedPayRunProfileStore(MemoryPayRunProfileStore inner, PayRunProfileCache cache, int hits, int misses) : IPayRunProfileStore
{
    public MemoryPayRunProfileStore Inner { get; set; } = inner;
    public PayRunProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunProfile? Find(string code)
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

    public void Save(PayRunProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run profiles. Empty fields match all.</summary>
public sealed record PayRunProfileQuery(string Title, double MinDiscount, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<PayRunProfile> Apply(List<PayRunProfile> items)
    {
        var matched = Collections.Filter(items, (PayRunProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run profile store.</summary>
public static class PayRunProfileStore
{
    public static IPayRunProfileStore NewMemoryPayRunProfileStore()
    {
        return new MemoryPayRunProfileStore(new Dictionary<string, PayRunProfile>(), new List<string>());
    }

    public static IPayRunProfileStore NewCachedPayRunProfileStore(MemoryPayRunProfileStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunProfileStore(inner, PayRunProfileCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PayRunProfile> FindPayRunProfiles(IPayRunProfileStore store, PayRunProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
