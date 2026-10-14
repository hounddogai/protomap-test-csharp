namespace Acme.Workforce.Assets.AssetReturn.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps asset return reservations by code.</summary>
public interface IAssetReturnReservationStore
{
    AssetReturnReservation? Find(string code);
    void Save(AssetReturnReservation item);
    bool Remove(string code);
    List<AssetReturnReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssetReturnReservationStore(Dictionary<string, AssetReturnReservation> items, List<string> order) : IAssetReturnReservationStore
{
    public Dictionary<string, AssetReturnReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssetReturnReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssetReturnReservation item)
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

    public List<AssetReturnReservation> List()
    {
        var outValue = new List<AssetReturnReservation>();
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
public sealed class CachedAssetReturnReservationStore(MemoryAssetReturnReservationStore inner, AssetReturnReservationCache cache, int hits, int misses) : IAssetReturnReservationStore
{
    public MemoryAssetReturnReservationStore Inner { get; set; } = inner;
    public AssetReturnReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssetReturnReservation? Find(string code)
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

    public void Save(AssetReturnReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssetReturnReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects asset return reservations. Empty fields match all.</summary>
public sealed record AssetReturnReservationQuery(string Department, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(AssetReturnReservation item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<AssetReturnReservation> Apply(List<AssetReturnReservation> items)
    {
        var matched = Collections.Filter(items, (AssetReturnReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of asset return reservation store.</summary>
public static class AssetReturnReservationStore
{
    public static IAssetReturnReservationStore NewMemoryAssetReturnReservationStore()
    {
        return new MemoryAssetReturnReservationStore(
            new Dictionary<string, AssetReturnReservation>(),
            new List<string>());
    }

    public static IAssetReturnReservationStore NewCachedAssetReturnReservationStore(MemoryAssetReturnReservationStore inner, int ttl, IClock clock)
    {
        return new CachedAssetReturnReservationStore(
            inner,
            AssetReturnReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<AssetReturnReservation> FindAssetReturnReservations(IAssetReturnReservationStore store, AssetReturnReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
