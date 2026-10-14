namespace Acme.Workforce.Compliance.TrainingDue.Quota;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional training due reservations by code.</summary>
public interface IRegionalTrainingDueReservationStore
{
    RegionalTrainingDueReservation? Find(string code);
    void Save(RegionalTrainingDueReservation item);
    bool Remove(string code);
    List<RegionalTrainingDueReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTrainingDueReservationStore(Dictionary<string, RegionalTrainingDueReservation> items, List<string> order) : IRegionalTrainingDueReservationStore
{
    public Dictionary<string, RegionalTrainingDueReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTrainingDueReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTrainingDueReservation item)
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

    public List<RegionalTrainingDueReservation> List()
    {
        var outValue = new List<RegionalTrainingDueReservation>();
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
public sealed class CachedRegionalTrainingDueReservationStore(MemoryRegionalTrainingDueReservationStore inner, RegionalTrainingDueReservationCache cache, int hits, int misses) : IRegionalTrainingDueReservationStore
{
    public MemoryRegionalTrainingDueReservationStore Inner { get; set; } = inner;
    public RegionalTrainingDueReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTrainingDueReservation? Find(string code)
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

    public void Save(RegionalTrainingDueReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTrainingDueReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional training due reservations. Empty fields match all.</summary>
public sealed record RegionalTrainingDueReservationQuery(string Category, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTrainingDueReservation item)
    {
        if (Category != "" && Text.Normalize(item.Category) != Text.Normalize(Category))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTrainingDueReservation> Apply(List<RegionalTrainingDueReservation> items)
    {
        var matched = Collections.Filter(items, (RegionalTrainingDueReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional training due reservation store.</summary>
public static class RegionalTrainingDueReservationStore
{
    public static IRegionalTrainingDueReservationStore NewMemoryRegionalTrainingDueReservationStore()
    {
        return new MemoryRegionalTrainingDueReservationStore(
            new Dictionary<string, RegionalTrainingDueReservation>(),
            new List<string>());
    }

    public static IRegionalTrainingDueReservationStore NewCachedRegionalTrainingDueReservationStore(MemoryRegionalTrainingDueReservationStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTrainingDueReservationStore(
            inner,
            RegionalTrainingDueReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTrainingDueReservation> FindRegionalTrainingDueReservations(IRegionalTrainingDueReservationStore store, RegionalTrainingDueReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
