namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps global deduction reservations by code.</summary>
public interface IGlobalDeductionReservationStore
{
    GlobalDeductionReservation? Find(string code);
    void Save(GlobalDeductionReservation item);
    bool Remove(string code);
    List<GlobalDeductionReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryGlobalDeductionReservationStore(Dictionary<string, GlobalDeductionReservation> items, List<string> order) : IGlobalDeductionReservationStore
{
    public Dictionary<string, GlobalDeductionReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public GlobalDeductionReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(GlobalDeductionReservation item)
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

    public List<GlobalDeductionReservation> List()
    {
        var outValue = new List<GlobalDeductionReservation>();
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
public sealed class CachedGlobalDeductionReservationStore(MemoryGlobalDeductionReservationStore inner, GlobalDeductionReservationCache cache, int hits, int misses) : IGlobalDeductionReservationStore
{
    public MemoryGlobalDeductionReservationStore Inner { get; set; } = inner;
    public GlobalDeductionReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public GlobalDeductionReservation? Find(string code)
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

    public void Save(GlobalDeductionReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<GlobalDeductionReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects global deduction reservations. Empty fields match all.</summary>
public sealed record GlobalDeductionReservationQuery(string Segment, double MinRating, List<string> Statuses, int Limit)
{
    public bool Matches(GlobalDeductionReservation item)
    {
        if (Segment != "" && Text.Normalize(item.Segment) != Text.Normalize(Segment))
        {
            return false;
        }
        if (item.Rating < MinRating)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<GlobalDeductionReservation> Apply(List<GlobalDeductionReservation> items)
    {
        var matched = Collections.Filter(items, (GlobalDeductionReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of global deduction reservation store.</summary>
public static class GlobalDeductionReservationStore
{
    public static IGlobalDeductionReservationStore NewMemoryGlobalDeductionReservationStore()
    {
        return new MemoryGlobalDeductionReservationStore(
            new Dictionary<string, GlobalDeductionReservation>(),
            new List<string>());
    }

    public static IGlobalDeductionReservationStore NewCachedGlobalDeductionReservationStore(MemoryGlobalDeductionReservationStore inner, int ttl, IClock clock)
    {
        return new CachedGlobalDeductionReservationStore(
            inner,
            GlobalDeductionReservationCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<GlobalDeductionReservation> FindGlobalDeductionReservations(IGlobalDeductionReservationStore store, GlobalDeductionReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
