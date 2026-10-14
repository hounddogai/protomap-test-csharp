namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default bonus pool reservations by code.</summary>
public interface IDefaultBonusPoolReservationStore
{
    DefaultBonusPoolReservation? Find(string code);
    void Save(DefaultBonusPoolReservation item);
    bool Remove(string code);
    List<DefaultBonusPoolReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultBonusPoolReservationStore(Dictionary<string, DefaultBonusPoolReservation> items, List<string> order) : IDefaultBonusPoolReservationStore
{
    public Dictionary<string, DefaultBonusPoolReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultBonusPoolReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultBonusPoolReservation item)
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

    public List<DefaultBonusPoolReservation> List()
    {
        var outValue = new List<DefaultBonusPoolReservation>();
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
public sealed class CachedDefaultBonusPoolReservationStore(MemoryDefaultBonusPoolReservationStore inner, DefaultBonusPoolReservationCache cache, int hits, int misses) : IDefaultBonusPoolReservationStore
{
    public MemoryDefaultBonusPoolReservationStore Inner { get; set; } = inner;
    public DefaultBonusPoolReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultBonusPoolReservation? Find(string code)
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

    public void Save(DefaultBonusPoolReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultBonusPoolReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default bonus pool reservations. Empty fields match all.</summary>
public sealed record DefaultBonusPoolReservationQuery(string Unit, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultBonusPoolReservation item)
    {
        if (Unit != "" && Text.Normalize(item.Unit) != Text.Normalize(Unit))
        {
            return false;
        }
        if (item.UpdatedAt < MinUpdatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultBonusPoolReservation> Apply(List<DefaultBonusPoolReservation> items)
    {
        var matched = Collections.Filter(items, (DefaultBonusPoolReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default bonus pool reservation store.</summary>
public static class DefaultBonusPoolReservationStore
{
    public static IDefaultBonusPoolReservationStore NewMemoryDefaultBonusPoolReservationStore()
    {
        return new MemoryDefaultBonusPoolReservationStore(
            new Dictionary<string, DefaultBonusPoolReservation>(),
            new List<string>());
    }

    public static IDefaultBonusPoolReservationStore NewCachedDefaultBonusPoolReservationStore(MemoryDefaultBonusPoolReservationStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultBonusPoolReservationStore(
            inner,
            DefaultBonusPoolReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultBonusPoolReservation> FindDefaultBonusPoolReservations(IDefaultBonusPoolReservationStore store, DefaultBonusPoolReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
