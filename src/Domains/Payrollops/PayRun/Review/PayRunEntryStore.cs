namespace Acme.Workforce.Payrollops.PayRun.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay run entries by code.</summary>
public interface IPayRunEntryStore
{
    PayRunEntry? Find(string code);
    void Save(PayRunEntry item);
    bool Remove(string code);
    List<PayRunEntry> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayRunEntryStore(Dictionary<string, PayRunEntry> items, List<string> order) : IPayRunEntryStore
{
    public Dictionary<string, PayRunEntry> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayRunEntry? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayRunEntry item)
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

    public List<PayRunEntry> List()
    {
        var outValue = new List<PayRunEntry>();
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
public sealed class CachedPayRunEntryStore(MemoryPayRunEntryStore inner, PayRunEntryCache cache, int hits, int misses) : IPayRunEntryStore
{
    public MemoryPayRunEntryStore Inner { get; set; } = inner;
    public PayRunEntryCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayRunEntry? Find(string code)
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

    public void Save(PayRunEntry item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayRunEntry> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay run entries. Empty fields match all.</summary>
public sealed record PayRunEntryQuery(string Title, double MinFloorValue, List<string> Statuses, int Limit)
{
    public bool Matches(PayRunEntry item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.FloorValue < MinFloorValue)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayRunEntry> Apply(List<PayRunEntry> items)
    {
        var matched = Collections.Filter(items, (PayRunEntry candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay run entry store.</summary>
public static class PayRunEntryStore
{
    public static IPayRunEntryStore NewMemoryPayRunEntryStore()
    {
        return new MemoryPayRunEntryStore(new Dictionary<string, PayRunEntry>(), new List<string>());
    }

    public static IPayRunEntryStore NewCachedPayRunEntryStore(MemoryPayRunEntryStore inner, int ttl, IClock clock)
    {
        return new CachedPayRunEntryStore(inner, PayRunEntryCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayRunEntry> FindPayRunEntries(IPayRunEntryStore store, PayRunEntryQuery query)
    {
        return query.Apply(store.List());
    }
}
