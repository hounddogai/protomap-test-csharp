namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band entries by code.</summary>
public interface IPayBandEntryStore
{
    PayBandEntry? Find(string code);
    void Save(PayBandEntry item);
    bool Remove(string code);
    List<PayBandEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandEntryStore(Dictionary<string, PayBandEntry> items, List<string> order) : IPayBandEntryStore
{
    public Dictionary<string, PayBandEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandEntry item)
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

    public List<PayBandEntry> List()
    {
        var outValue = new List<PayBandEntry>();
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
public sealed class CachedPayBandEntryStore(MemoryPayBandEntryStore inner, PayBandEntryCache cache, int hits, int misses) : IPayBandEntryStore
{
    public MemoryPayBandEntryStore Inner { get; set; } = inner;
    public PayBandEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandEntry? Find(string code)
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

    public void Save(PayBandEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band entries. Empty fields match all.</summary>
public sealed record PayBandEntryQuery(string Department, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandEntry item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandEntry> Apply(List<PayBandEntry> items)
    {
        var matched = Collections.Filter(items, (PayBandEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band entry store.</summary>
public static class PayBandEntryStore
{
    public static IPayBandEntryStore NewMemoryPayBandEntryStore()
    {
        return new MemoryPayBandEntryStore(new Dictionary<string, PayBandEntry>(), new List<string>());
    }

    public static IPayBandEntryStore NewCachedPayBandEntryStore(MemoryPayBandEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandEntryStore(inner, PayBandEntryCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandEntry> FindPayBandEntries(IPayBandEntryStore store, PayBandEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
