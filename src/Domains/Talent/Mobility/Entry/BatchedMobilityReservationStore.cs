namespace Acme.Workforce.Talent.Mobility.Entry;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched mobility reservations by code.</summary>
public interface IBatchedMobilityReservationStore
{
    BatchedMobilityReservation? Find(string code);
    void Save(BatchedMobilityReservation item);
    bool Remove(string code);
    List<BatchedMobilityReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedMobilityReservationStore(Dictionary<string, BatchedMobilityReservation> items, List<string> order) : IBatchedMobilityReservationStore
{
    public Dictionary<string, BatchedMobilityReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedMobilityReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedMobilityReservation item)
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

    public List<BatchedMobilityReservation> List()
    {
        var outValue = new List<BatchedMobilityReservation>();
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
public sealed class CachedBatchedMobilityReservationStore(MemoryBatchedMobilityReservationStore inner, BatchedMobilityReservationCache cache, int hits, int misses) : IBatchedMobilityReservationStore
{
    public MemoryBatchedMobilityReservationStore Inner { get; set; } = inner;
    public BatchedMobilityReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedMobilityReservation? Find(string code)
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

    public void Save(BatchedMobilityReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedMobilityReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched mobility reservations. Empty fields match all.</summary>
public sealed record BatchedMobilityReservationQuery(string Locale, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedMobilityReservation item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
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

    public List<BatchedMobilityReservation> Apply(List<BatchedMobilityReservation> items)
    {
        var matched = Collections.Filter(items, (BatchedMobilityReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched mobility reservation store.</summary>
public static class BatchedMobilityReservationStore
{
    public static IBatchedMobilityReservationStore NewMemoryBatchedMobilityReservationStore()
    {
        return new MemoryBatchedMobilityReservationStore(
            new Dictionary<string, BatchedMobilityReservation>(),
            new List<string>());
    }

    public static IBatchedMobilityReservationStore NewCachedBatchedMobilityReservationStore(MemoryBatchedMobilityReservationStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedMobilityReservationStore(
            inner,
            BatchedMobilityReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedMobilityReservation> FindBatchedMobilityReservations(IBatchedMobilityReservationStore store, BatchedMobilityReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
