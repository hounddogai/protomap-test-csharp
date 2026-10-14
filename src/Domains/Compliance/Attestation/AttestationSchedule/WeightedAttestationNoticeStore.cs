namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps weighted attestation notices by code.</summary>
public interface IWeightedAttestationNoticeStore
{
    WeightedAttestationNotice? Find(string code);
    void Save(WeightedAttestationNotice item);
    bool Remove(string code);
    List<WeightedAttestationNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryWeightedAttestationNoticeStore(Dictionary<string, WeightedAttestationNotice> items, List<string> order) : IWeightedAttestationNoticeStore
{
    public Dictionary<string, WeightedAttestationNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public WeightedAttestationNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(WeightedAttestationNotice item)
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

    public List<WeightedAttestationNotice> List()
    {
        var outValue = new List<WeightedAttestationNotice>();
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
public sealed class CachedWeightedAttestationNoticeStore(MemoryWeightedAttestationNoticeStore inner, WeightedAttestationNoticeCache cache, int hits, int misses) : IWeightedAttestationNoticeStore
{
    public MemoryWeightedAttestationNoticeStore Inner { get; set; } = inner;
    public WeightedAttestationNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public WeightedAttestationNotice? Find(string code)
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

    public void Save(WeightedAttestationNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<WeightedAttestationNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects weighted attestation notices. Empty fields match all.</summary>
public sealed record WeightedAttestationNoticeQuery(string Channel, double MinRate, List<string> Statuses, int Limit)
{
    public bool Matches(WeightedAttestationNotice item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Rate < MinRate)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<WeightedAttestationNotice> Apply(List<WeightedAttestationNotice> items)
    {
        var matched = Collections.Filter(items, (WeightedAttestationNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of weighted attestation notice store.</summary>
public static class WeightedAttestationNoticeStore
{
    public static IWeightedAttestationNoticeStore NewMemoryWeightedAttestationNoticeStore()
    {
        return new MemoryWeightedAttestationNoticeStore(
            new Dictionary<string, WeightedAttestationNotice>(),
            new List<string>());
    }

    public static IWeightedAttestationNoticeStore NewCachedWeightedAttestationNoticeStore(MemoryWeightedAttestationNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedWeightedAttestationNoticeStore(
            inner,
            WeightedAttestationNoticeCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<WeightedAttestationNotice> FindWeightedAttestationNotices(IWeightedAttestationNoticeStore store, WeightedAttestationNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
