namespace Acme.Workforce.Contractors.Timesheet.Group;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary timesheet groups by code.</summary>
public interface IPrimaryTimesheetGroupStore
{
    PrimaryTimesheetGroup? Find(string code);
    void Save(PrimaryTimesheetGroup item);
    bool Remove(string code);
    List<PrimaryTimesheetGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryTimesheetGroupStore(Dictionary<string, PrimaryTimesheetGroup> items, List<string> order) : IPrimaryTimesheetGroupStore
{
    public Dictionary<string, PrimaryTimesheetGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryTimesheetGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryTimesheetGroup item)
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

    public List<PrimaryTimesheetGroup> List()
    {
        var outValue = new List<PrimaryTimesheetGroup>();
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
public sealed class CachedPrimaryTimesheetGroupStore(MemoryPrimaryTimesheetGroupStore inner, PrimaryTimesheetGroupCache cache, int hits, int misses) : IPrimaryTimesheetGroupStore
{
    public MemoryPrimaryTimesheetGroupStore Inner { get; set; } = inner;
    public PrimaryTimesheetGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryTimesheetGroup? Find(string code)
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

    public void Save(PrimaryTimesheetGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryTimesheetGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary timesheet groups. Empty fields match all.</summary>
public sealed record PrimaryTimesheetGroupQuery(string Country, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryTimesheetGroup item)
    {
        if (Country != "" && Text.Normalize(item.Country) != Text.Normalize(Country))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryTimesheetGroup> Apply(List<PrimaryTimesheetGroup> items)
    {
        var matched = Collections.Filter(items, (PrimaryTimesheetGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary timesheet group store.</summary>
public static class PrimaryTimesheetGroupStore
{
    public static IPrimaryTimesheetGroupStore NewMemoryPrimaryTimesheetGroupStore()
    {
        return new MemoryPrimaryTimesheetGroupStore(
            new Dictionary<string, PrimaryTimesheetGroup>(),
            new List<string>());
    }

    public static IPrimaryTimesheetGroupStore NewCachedPrimaryTimesheetGroupStore(MemoryPrimaryTimesheetGroupStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryTimesheetGroupStore(inner, PrimaryTimesheetGroupCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<PrimaryTimesheetGroup> FindPrimaryTimesheetGroups(IPrimaryTimesheetGroupStore store, PrimaryTimesheetGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
