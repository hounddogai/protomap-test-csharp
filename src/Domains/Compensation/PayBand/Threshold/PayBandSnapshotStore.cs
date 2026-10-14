namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band snapshots by code.</summary>
public interface IPayBandSnapshotStore
{
    PayBandSnapshot? Find(string code);
    void Save(PayBandSnapshot item);
    bool Remove(string code);
    List<PayBandSnapshot> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandSnapshotStore(Dictionary<string, PayBandSnapshot> items, List<string> order) : IPayBandSnapshotStore
{
    public Dictionary<string, PayBandSnapshot> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandSnapshot? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandSnapshot item)
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

    public List<PayBandSnapshot> List()
    {
        var outValue = new List<PayBandSnapshot>();
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
public sealed class CachedPayBandSnapshotStore(MemoryPayBandSnapshotStore inner, PayBandSnapshotCache cache, int hits, int misses) : IPayBandSnapshotStore
{
    public MemoryPayBandSnapshotStore Inner { get; set; } = inner;
    public PayBandSnapshotCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandSnapshot? Find(string code)
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

    public void Save(PayBandSnapshot item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandSnapshot> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band snapshots. Empty fields match all.</summary>
public sealed record PayBandSnapshotQuery(string Category, int MinQuantity, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandSnapshot item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Quantity < MinQuantity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandSnapshot> Apply(List<PayBandSnapshot> items)
    {
        var matched = Collections.Filter(items, (PayBandSnapshot candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band snapshot store.</summary>
public static class PayBandSnapshotStore
{
    public static IPayBandSnapshotStore NewMemoryPayBandSnapshotStore()
    {
        return new MemoryPayBandSnapshotStore(new Dictionary<string, PayBandSnapshot>(), new List<string>());
    }

    public static IPayBandSnapshotStore NewCachedPayBandSnapshotStore(MemoryPayBandSnapshotStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandSnapshotStore(inner, PayBandSnapshotCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PayBandSnapshot> FindPayBandSnapshots(IPayBandSnapshotStore store, PayBandSnapshotQuery query)
    {
        return query.Apply(store.List());
    }
}
