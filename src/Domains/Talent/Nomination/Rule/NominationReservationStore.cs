namespace Acme.Workforce.Talent.Nomination.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps nomination reservations by code.</summary>
public interface INominationReservationStore
{
    NominationReservation? Find(string code);
    void Save(NominationReservation item);
    bool Remove(string code);
    List<NominationReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryNominationReservationStore(Dictionary<string, NominationReservation> items, List<string> order) : INominationReservationStore
{
    public Dictionary<string, NominationReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public NominationReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(NominationReservation item)
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

    public List<NominationReservation> List()
    {
        var outValue = new List<NominationReservation>();
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
public sealed class CachedNominationReservationStore(MemoryNominationReservationStore inner, NominationReservationCache cache, int hits, int misses) : INominationReservationStore
{
    public MemoryNominationReservationStore Inner { get; set; } = inner;
    public NominationReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public NominationReservation? Find(string code)
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

    public void Save(NominationReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<NominationReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects nomination reservations. Empty fields match all.</summary>
public sealed record NominationReservationQuery(string Segment, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(NominationReservation item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<NominationReservation> Apply(List<NominationReservation> items)
    {
        var matched = Collections.Filter(items, (NominationReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of nomination reservation store.</summary>
public static class NominationReservationStore
{
    public static INominationReservationStore NewMemoryNominationReservationStore()
    {
        return new MemoryNominationReservationStore(
            new Dictionary<string, NominationReservation>(),
            new List<string>());
    }

    public static INominationReservationStore NewCachedNominationReservationStore(MemoryNominationReservationStore inner, int ttl, IClock clock)
    {
        return new CachedNominationReservationStore(inner, NominationReservationCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<NominationReservation> FindNominationReservations(INominationReservationStore store, NominationReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
