namespace Acme.Workforce.Compensation.PayBand.Draft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band drafts by code.</summary>
public interface IPayBandDraftStore
{
    PayBandDraft? Find(string code);
    void Save(PayBandDraft item);
    bool Remove(string code);
    List<PayBandDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandDraftStore(Dictionary<string, PayBandDraft> items, List<string> order) : IPayBandDraftStore
{
    public Dictionary<string, PayBandDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandDraft item)
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

    public List<PayBandDraft> List()
    {
        var outValue = new List<PayBandDraft>();
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
public sealed class CachedPayBandDraftStore(MemoryPayBandDraftStore inner, PayBandDraftCache cache, int hits, int misses) : IPayBandDraftStore
{
    public MemoryPayBandDraftStore Inner { get; set; } = inner;
    public PayBandDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandDraft? Find(string code)
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

    public void Save(PayBandDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band drafts. Empty fields match all.</summary>
public sealed record PayBandDraftQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandDraft item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandDraft> Apply(List<PayBandDraft> items)
    {
        var matched = Collections.Filter(items, (PayBandDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band draft store.</summary>
public static class PayBandDraftStore
{
    public static IPayBandDraftStore NewMemoryPayBandDraftStore()
    {
        return new MemoryPayBandDraftStore(new Dictionary<string, PayBandDraft>(), new List<string>());
    }

    public static IPayBandDraftStore NewCachedPayBandDraftStore(MemoryPayBandDraftStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandDraftStore(inner, PayBandDraftCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayBandDraft> FindPayBandDrafts(IPayBandDraftStore store, PayBandDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
