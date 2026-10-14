namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps batched nomination reservations by code.</summary>
public interface IBatchedNominationReservationStore
{
    BatchedNominationReservation? Find(string code);
    void Save(BatchedNominationReservation item);
    bool Remove(string code);
    List<BatchedNominationReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBatchedNominationReservationStore(Dictionary<string, BatchedNominationReservation> items, List<string> order) : IBatchedNominationReservationStore
{
    public Dictionary<string, BatchedNominationReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BatchedNominationReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BatchedNominationReservation item)
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

    public List<BatchedNominationReservation> List()
    {
        var outValue = new List<BatchedNominationReservation>();
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
public sealed class CachedBatchedNominationReservationStore(MemoryBatchedNominationReservationStore inner, BatchedNominationReservationCache cache, int hits, int misses) : IBatchedNominationReservationStore
{
    public MemoryBatchedNominationReservationStore Inner { get; set; } = inner;
    public BatchedNominationReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BatchedNominationReservation? Find(string code)
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

    public void Save(BatchedNominationReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BatchedNominationReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects batched nomination reservations. Empty fields match all.</summary>
public sealed record BatchedNominationReservationQuery(string Title, int MinUpdatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BatchedNominationReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BatchedNominationReservation> Apply(List<BatchedNominationReservation> items)
    {
        var matched = Collections.Filter(items, (BatchedNominationReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of batched nomination reservation store.</summary>
public static class BatchedNominationReservationStore
{
    public static IBatchedNominationReservationStore NewMemoryBatchedNominationReservationStore()
    {
        return new MemoryBatchedNominationReservationStore(
            new Dictionary<string, BatchedNominationReservation>(),
            new List<string>());
    }

    public static IBatchedNominationReservationStore NewCachedBatchedNominationReservationStore(MemoryBatchedNominationReservationStore inner, int ttl, IClock clock)
    {
        return new CachedBatchedNominationReservationStore(
            inner,
            BatchedNominationReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<BatchedNominationReservation> FindBatchedNominationReservations(IBatchedNominationReservationStore store, BatchedNominationReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
