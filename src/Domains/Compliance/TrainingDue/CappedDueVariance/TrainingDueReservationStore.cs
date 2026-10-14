namespace Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due reservations by code.</summary>
public interface ITrainingDueReservationStore
{
    TrainingDueReservation? Find(string code);
    void Save(TrainingDueReservation item);
    bool Remove(string code);
    List<TrainingDueReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueReservationStore(Dictionary<string, TrainingDueReservation> items, List<string> order) : ITrainingDueReservationStore
{
    public Dictionary<string, TrainingDueReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueReservation item)
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

    public List<TrainingDueReservation> List()
    {
        var outValue = new List<TrainingDueReservation>();
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
public sealed class CachedTrainingDueReservationStore(MemoryTrainingDueReservationStore inner, TrainingDueReservationCache cache, int hits, int misses) : ITrainingDueReservationStore
{
    public MemoryTrainingDueReservationStore Inner { get; set; } = inner;
    public TrainingDueReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueReservation? Find(string code)
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

    public void Save(TrainingDueReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due reservations. Empty fields match all.</summary>
public sealed record TrainingDueReservationQuery(string Team, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueReservation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<TrainingDueReservation> Apply(List<TrainingDueReservation> items)
    {
        var matched = Collections.Filter(items, (TrainingDueReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due reservation store.</summary>
public static class TrainingDueReservationStore
{
    public static ITrainingDueReservationStore NewMemoryTrainingDueReservationStore()
    {
        return new MemoryTrainingDueReservationStore(
            new Dictionary<string, TrainingDueReservation>(),
            new List<string>());
    }

    public static ITrainingDueReservationStore NewCachedTrainingDueReservationStore(MemoryTrainingDueReservationStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueReservationStore(
            inner,
            TrainingDueReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TrainingDueReservation> FindTrainingDueReservations(ITrainingDueReservationStore store, TrainingDueReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
