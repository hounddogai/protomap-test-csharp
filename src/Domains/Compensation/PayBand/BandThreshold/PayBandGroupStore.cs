namespace Acme.Workforce.Compensation.PayBand.BandThreshold;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps pay band groups by code.</summary>
public interface IPayBandGroupStore
{
    PayBandGroup? Find(string code);
    void Save(PayBandGroup item);
    bool Remove(string code);
    List<PayBandGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayBandGroupStore(Dictionary<string, PayBandGroup> items, List<string> order) : IPayBandGroupStore
{
    public Dictionary<string, PayBandGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayBandGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayBandGroup item)
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

    public List<PayBandGroup> List()
    {
        var outValue = new List<PayBandGroup>();
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
public sealed class CachedPayBandGroupStore(MemoryPayBandGroupStore inner, PayBandGroupCache cache, int hits, int misses) : IPayBandGroupStore
{
    public MemoryPayBandGroupStore Inner { get; set; } = inner;
    public PayBandGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayBandGroup? Find(string code)
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

    public void Save(PayBandGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayBandGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects pay band groups. Empty fields match all.</summary>
public sealed record PayBandGroupQuery(string Channel, int MinBatchSize, List<string> Statuses, int Limit)
{
    public bool Matches(PayBandGroup item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.BatchSize < MinBatchSize)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayBandGroup> Apply(List<PayBandGroup> items)
    {
        var matched = Collections.Filter(items, (PayBandGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of pay band group store.</summary>
public static class PayBandGroupStore
{
    public static IPayBandGroupStore NewMemoryPayBandGroupStore()
    {
        return new MemoryPayBandGroupStore(new Dictionary<string, PayBandGroup>(), new List<string>());
    }

    public static IPayBandGroupStore NewCachedPayBandGroupStore(MemoryPayBandGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPayBandGroupStore(inner, PayBandGroupCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PayBandGroup> FindPayBandGroups(IPayBandGroupStore store, PayBandGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
