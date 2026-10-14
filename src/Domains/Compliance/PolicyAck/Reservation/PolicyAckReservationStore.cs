namespace Acme.Workforce.Compliance.PolicyAck.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy ack reservations by code.</summary>
public interface IPolicyAckReservationStore
{
    PolicyAckReservation? Find(string code);
    void Save(PolicyAckReservation item);
    bool Remove(string code);
    List<PolicyAckReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyAckReservationStore(Dictionary<string, PolicyAckReservation> items, List<string> order) : IPolicyAckReservationStore
{
    public Dictionary<string, PolicyAckReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyAckReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyAckReservation item)
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

    public List<PolicyAckReservation> List()
    {
        var outValue = new List<PolicyAckReservation>();
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
public sealed class CachedPolicyAckReservationStore(MemoryPolicyAckReservationStore inner, PolicyAckReservationCache cache, int hits, int misses) : IPolicyAckReservationStore
{
    public MemoryPolicyAckReservationStore Inner { get; set; } = inner;
    public PolicyAckReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyAckReservation? Find(string code)
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

    public void Save(PolicyAckReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyAckReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy ack reservations. Empty fields match all.</summary>
public sealed record PolicyAckReservationQuery(string Title, double MinHours, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyAckReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Hours < MinHours)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyAckReservation> Apply(List<PolicyAckReservation> items)
    {
        var matched = Collections.Filter(items, (PolicyAckReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy ack reservation store.</summary>
public static class PolicyAckReservationStore
{
    public static IPolicyAckReservationStore NewMemoryPolicyAckReservationStore()
    {
        return new MemoryPolicyAckReservationStore(
            new Dictionary<string, PolicyAckReservation>(),
            new List<string>());
    }

    public static IPolicyAckReservationStore NewCachedPolicyAckReservationStore(MemoryPolicyAckReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyAckReservationStore(inner, PolicyAckReservationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<PolicyAckReservation> FindPolicyAckReservations(IPolicyAckReservationStore store, PolicyAckReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
