namespace Acme.Workforce.Assets.AssetReturn.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional asset return reservations by code.</summary>
public interface IRegionalAssetReturnReservationStore
{
    RegionalAssetReturnReservation? Find(string code);
    void Save(RegionalAssetReturnReservation item);
    bool Remove(string code);
    List<RegionalAssetReturnReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalAssetReturnReservationStore(Dictionary<string, RegionalAssetReturnReservation> items, List<string> order) : IRegionalAssetReturnReservationStore
{
    public Dictionary<string, RegionalAssetReturnReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalAssetReturnReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalAssetReturnReservation item)
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

    public List<RegionalAssetReturnReservation> List()
    {
        var outValue = new List<RegionalAssetReturnReservation>();
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
public sealed class CachedRegionalAssetReturnReservationStore(MemoryRegionalAssetReturnReservationStore inner, RegionalAssetReturnReservationCache cache, int hits, int misses) : IRegionalAssetReturnReservationStore
{
    public MemoryRegionalAssetReturnReservationStore Inner { get; set; } = inner;
    public RegionalAssetReturnReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalAssetReturnReservation? Find(string code)
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

    public void Save(RegionalAssetReturnReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalAssetReturnReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional asset return reservations. Empty fields match all.</summary>
public sealed record RegionalAssetReturnReservationQuery(string Country, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalAssetReturnReservation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalAssetReturnReservation> Apply(List<RegionalAssetReturnReservation> items)
    {
        var matched = Collections.Filter(items, (RegionalAssetReturnReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional asset return reservation store.</summary>
public static class RegionalAssetReturnReservationStore
{
    public static IRegionalAssetReturnReservationStore NewMemoryRegionalAssetReturnReservationStore()
    {
        return new MemoryRegionalAssetReturnReservationStore(
            new Dictionary<string, RegionalAssetReturnReservation>(),
            new List<string>());
    }

    public static IRegionalAssetReturnReservationStore NewCachedRegionalAssetReturnReservationStore(MemoryRegionalAssetReturnReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalAssetReturnReservationStore(
            inner,
            RegionalAssetReturnReservationCache.Create(128, ttl, clock),
            0,
            0);
    }

    public static List<RegionalAssetReturnReservation> FindRegionalAssetReturnReservations(IRegionalAssetReturnReservationStore store, RegionalAssetReturnReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
