namespace Acme.Workforce.Talent.SuccessionPlan.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps succession plan reservations by code.</summary>
public interface ISuccessionPlanReservationStore
{
    SuccessionPlanReservation? Find(string code);
    void Save(SuccessionPlanReservation item);
    bool Remove(string code);
    List<SuccessionPlanReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemorySuccessionPlanReservationStore(Dictionary<string, SuccessionPlanReservation> items, List<string> order) : ISuccessionPlanReservationStore
{
    public Dictionary<string, SuccessionPlanReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public SuccessionPlanReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(SuccessionPlanReservation item)
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

    public List<SuccessionPlanReservation> List()
    {
        var outValue = new List<SuccessionPlanReservation>();
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
public sealed class CachedSuccessionPlanReservationStore(MemorySuccessionPlanReservationStore inner, SuccessionPlanReservationCache cache, int hits, int misses) : ISuccessionPlanReservationStore
{
    public MemorySuccessionPlanReservationStore Inner { get; set; } = inner;
    public SuccessionPlanReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public SuccessionPlanReservation? Find(string code)
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

    public void Save(SuccessionPlanReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<SuccessionPlanReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects succession plan reservations. Empty fields match all.</summary>
public sealed record SuccessionPlanReservationQuery(string Team, double MinMaximum, List<string> Statuses, int Limit)
{
    public bool Matches(SuccessionPlanReservation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Maximum < MinMaximum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<SuccessionPlanReservation> Apply(List<SuccessionPlanReservation> items)
    {
        var matched = Collections.Filter(items, (SuccessionPlanReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of succession plan reservation store.</summary>
public static class SuccessionPlanReservationStore
{
    public static ISuccessionPlanReservationStore NewMemorySuccessionPlanReservationStore()
    {
        return new MemorySuccessionPlanReservationStore(
            new Dictionary<string, SuccessionPlanReservation>(),
            new List<string>());
    }

    public static ISuccessionPlanReservationStore NewCachedSuccessionPlanReservationStore(MemorySuccessionPlanReservationStore inner, int ttl, IClock clock)
    {
        return new CachedSuccessionPlanReservationStore(
            inner,
            SuccessionPlanReservationCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<SuccessionPlanReservation> FindSuccessionPlanReservations(ISuccessionPlanReservationStore store, SuccessionPlanReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
