namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps policy reservations by code.</summary>
public interface IPolicyReservationStore
{
    PolicyReservation? Find(string code);
    void Save(PolicyReservation item);
    bool Remove(string code);
    List<PolicyReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPolicyReservationStore(Dictionary<string, PolicyReservation> items, List<string> order) : IPolicyReservationStore
{
    public Dictionary<string, PolicyReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PolicyReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PolicyReservation item)
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

    public List<PolicyReservation> List()
    {
        var outValue = new List<PolicyReservation>();
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
public sealed class CachedPolicyReservationStore(MemoryPolicyReservationStore inner, PolicyReservationCache cache, int hits, int misses) : IPolicyReservationStore
{
    public MemoryPolicyReservationStore Inner { get; set; } = inner;
    public PolicyReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PolicyReservation? Find(string code)
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

    public void Save(PolicyReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PolicyReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects policy reservations. Empty fields match all.</summary>
public sealed record PolicyReservationQuery(string Country, double MinMargin, List<string> Statuses, int Limit)
{
    public bool Matches(PolicyReservation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.Margin < MinMargin)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PolicyReservation> Apply(List<PolicyReservation> items)
    {
        var matched = Collections.Filter(items, (PolicyReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of policy reservation store.</summary>
public static class PolicyReservationStore
{
    public static IPolicyReservationStore NewMemoryPolicyReservationStore()
    {
        return new MemoryPolicyReservationStore(new Dictionary<string, PolicyReservation>(), new List<string>());
    }

    public static IPolicyReservationStore NewCachedPolicyReservationStore(MemoryPolicyReservationStore inner, int ttl, IClock clock)
    {
        return new CachedPolicyReservationStore(inner, PolicyReservationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<PolicyReservation> FindPolicyReservations(IPolicyReservationStore store, PolicyReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
