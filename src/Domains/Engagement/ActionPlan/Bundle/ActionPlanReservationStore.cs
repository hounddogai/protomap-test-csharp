namespace Acme.Workforce.Engagement.ActionPlan.Bundle;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps action plan reservations by code.</summary>
public interface IActionPlanReservationStore
{
    ActionPlanReservation? Find(string code);
    void Save(ActionPlanReservation item);
    bool Remove(string code);
    List<ActionPlanReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryActionPlanReservationStore(Dictionary<string, ActionPlanReservation> items, List<string> order) : IActionPlanReservationStore
{
    public Dictionary<string, ActionPlanReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ActionPlanReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ActionPlanReservation item)
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

    public List<ActionPlanReservation> List()
    {
        var outValue = new List<ActionPlanReservation>();
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
public sealed class CachedActionPlanReservationStore(MemoryActionPlanReservationStore inner, ActionPlanReservationCache cache, int hits, int misses) : IActionPlanReservationStore
{
    public MemoryActionPlanReservationStore Inner { get; set; } = inner;
    public ActionPlanReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ActionPlanReservation? Find(string code)
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

    public void Save(ActionPlanReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ActionPlanReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects action plan reservations. Empty fields match all.</summary>
public sealed record ActionPlanReservationQuery(string Country, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ActionPlanReservation item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
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

    public List<ActionPlanReservation> Apply(List<ActionPlanReservation> items)
    {
        var matched = Collections.Filter(items, (ActionPlanReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of action plan reservation store.</summary>
public static class ActionPlanReservationStore
{
    public static IActionPlanReservationStore NewMemoryActionPlanReservationStore()
    {
        return new MemoryActionPlanReservationStore(
            new Dictionary<string, ActionPlanReservation>(),
            new List<string>());
    }

    public static IActionPlanReservationStore NewCachedActionPlanReservationStore(MemoryActionPlanReservationStore inner, int ttl, IClock clock)
    {
        return new CachedActionPlanReservationStore(inner, ActionPlanReservationCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<ActionPlanReservation> FindActionPlanReservations(IActionPlanReservationStore store, ActionPlanReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
