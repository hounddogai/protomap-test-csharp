namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary timesheet adjustments by code.</summary>
public interface IPrimaryTimesheetAdjustmentStore
{
    PrimaryTimesheetAdjustment? Find(string code);
    void Save(PrimaryTimesheetAdjustment item);
    bool Remove(string code);
    List<PrimaryTimesheetAdjustment> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryTimesheetAdjustmentStore(Dictionary<string, PrimaryTimesheetAdjustment> items, List<string> order) : IPrimaryTimesheetAdjustmentStore
{
    public Dictionary<string, PrimaryTimesheetAdjustment> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryTimesheetAdjustment? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryTimesheetAdjustment item)
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

    public List<PrimaryTimesheetAdjustment> List()
    {
        var outValue = new List<PrimaryTimesheetAdjustment>();
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
public sealed class CachedPrimaryTimesheetAdjustmentStore(MemoryPrimaryTimesheetAdjustmentStore inner, PrimaryTimesheetAdjustmentCache cache, int hits, int misses) : IPrimaryTimesheetAdjustmentStore
{
    public MemoryPrimaryTimesheetAdjustmentStore Inner { get; set; } = inner;
    public PrimaryTimesheetAdjustmentCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryTimesheetAdjustment? Find(string code)
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

    public void Save(PrimaryTimesheetAdjustment item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryTimesheetAdjustment> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary timesheet adjustments. Empty fields match all.</summary>
public sealed record PrimaryTimesheetAdjustmentQuery(string Department, int MinDurationMinutes, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryTimesheetAdjustment item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.DurationMinutes < MinDurationMinutes)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryTimesheetAdjustment> Apply(List<PrimaryTimesheetAdjustment> items)
    {
        var matched = Collections.Filter(items, (PrimaryTimesheetAdjustment candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary timesheet adjustment store.</summary>
public static class PrimaryTimesheetAdjustmentStore
{
    public static IPrimaryTimesheetAdjustmentStore NewMemoryPrimaryTimesheetAdjustmentStore()
    {
        return new MemoryPrimaryTimesheetAdjustmentStore(
            new Dictionary<string, PrimaryTimesheetAdjustment>(),
            new List<string>());
    }

    public static IPrimaryTimesheetAdjustmentStore NewCachedPrimaryTimesheetAdjustmentStore(MemoryPrimaryTimesheetAdjustmentStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryTimesheetAdjustmentStore(
            inner,
            PrimaryTimesheetAdjustmentCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryTimesheetAdjustment> FindPrimaryTimesheetAdjustments(IPrimaryTimesheetAdjustmentStore store, PrimaryTimesheetAdjustmentQuery query)
    {
        return query.Apply(store.List());
    }
}
