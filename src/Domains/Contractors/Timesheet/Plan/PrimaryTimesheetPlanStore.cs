namespace Acme.Workforce.Contractors.Timesheet.Plan;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary timesheet plans by code.</summary>
public interface IPrimaryTimesheetPlanStore
{
    PrimaryTimesheetPlan? Find(string code);
    void Save(PrimaryTimesheetPlan item);
    bool Remove(string code);
    List<PrimaryTimesheetPlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryTimesheetPlanStore(Dictionary<string, PrimaryTimesheetPlan> items, List<string> order) : IPrimaryTimesheetPlanStore
{
    public Dictionary<string, PrimaryTimesheetPlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryTimesheetPlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryTimesheetPlan item)
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

    public List<PrimaryTimesheetPlan> List()
    {
        var outValue = new List<PrimaryTimesheetPlan>();
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
public sealed class CachedPrimaryTimesheetPlanStore(MemoryPrimaryTimesheetPlanStore inner, PrimaryTimesheetPlanCache cache, int hits, int misses) : IPrimaryTimesheetPlanStore
{
    public MemoryPrimaryTimesheetPlanStore Inner { get; set; } = inner;
    public PrimaryTimesheetPlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryTimesheetPlan? Find(string code)
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

    public void Save(PrimaryTimesheetPlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryTimesheetPlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary timesheet plans. Empty fields match all.</summary>
public sealed record PrimaryTimesheetPlanQuery(string Channel, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryTimesheetPlan item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
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

    public List<PrimaryTimesheetPlan> Apply(List<PrimaryTimesheetPlan> items)
    {
        var matched = Collections.Filter(items, (PrimaryTimesheetPlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary timesheet plan store.</summary>
public static class PrimaryTimesheetPlanStore
{
    public static IPrimaryTimesheetPlanStore NewMemoryPrimaryTimesheetPlanStore()
    {
        return new MemoryPrimaryTimesheetPlanStore(
            new Dictionary<string, PrimaryTimesheetPlan>(),
            new List<string>());
    }

    public static IPrimaryTimesheetPlanStore NewCachedPrimaryTimesheetPlanStore(MemoryPrimaryTimesheetPlanStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryTimesheetPlanStore(inner, PrimaryTimesheetPlanCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PrimaryTimesheetPlan> FindPrimaryTimesheetPlans(IPrimaryTimesheetPlanStore store, PrimaryTimesheetPlanQuery query)
    {
        return query.Apply(store.List());
    }
}
