namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps tiered action plan reservations by code.</summary>
public interface ITieredActionPlanReservationStore
{
    TieredActionPlanReservation? Find(string code);
    void Save(TieredActionPlanReservation item);
    bool Remove(string code);
    List<TieredActionPlanReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTieredActionPlanReservationStore(Dictionary<string, TieredActionPlanReservation> items, List<string> order) : ITieredActionPlanReservationStore
{
    public Dictionary<string, TieredActionPlanReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TieredActionPlanReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TieredActionPlanReservation item)
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

    public List<TieredActionPlanReservation> List()
    {
        var outValue = new List<TieredActionPlanReservation>();
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
public sealed class CachedTieredActionPlanReservationStore(MemoryTieredActionPlanReservationStore inner, TieredActionPlanReservationCache cache, int hits, int misses) : ITieredActionPlanReservationStore
{
    public MemoryTieredActionPlanReservationStore Inner { get; set; } = inner;
    public TieredActionPlanReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TieredActionPlanReservation? Find(string code)
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

    public void Save(TieredActionPlanReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TieredActionPlanReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects tiered action plan reservations. Empty fields match all.</summary>
public sealed record TieredActionPlanReservationQuery(string Team, int MinRetries, List<string> Statuses, int Limit)
{
    public bool Matches(TieredActionPlanReservation item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Retries < MinRetries)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TieredActionPlanReservation> Apply(List<TieredActionPlanReservation> items)
    {
        var matched = Collections.Filter(items, (TieredActionPlanReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of tiered action plan reservation store.</summary>
public static class TieredActionPlanReservationStore
{
    public static ITieredActionPlanReservationStore NewMemoryTieredActionPlanReservationStore()
    {
        return new MemoryTieredActionPlanReservationStore(
            new Dictionary<string, TieredActionPlanReservation>(),
            new List<string>());
    }

    public static ITieredActionPlanReservationStore NewCachedTieredActionPlanReservationStore(MemoryTieredActionPlanReservationStore inner, int ttl, IClock clock)
    {
        return new CachedTieredActionPlanReservationStore(
            inner,
            TieredActionPlanReservationCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<TieredActionPlanReservation> FindTieredActionPlanReservations(ITieredActionPlanReservationStore store, TieredActionPlanReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
