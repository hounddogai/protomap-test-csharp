namespace Acme.Workforce.Contractors.Timesheet.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary timesheet exceptions by code.</summary>
public interface IPrimaryTimesheetExceptionStore
{
    PrimaryTimesheetException? Find(string code);
    void Save(PrimaryTimesheetException item);
    bool Remove(string code);
    List<PrimaryTimesheetException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryTimesheetExceptionStore(Dictionary<string, PrimaryTimesheetException> items, List<string> order) : IPrimaryTimesheetExceptionStore
{
    public Dictionary<string, PrimaryTimesheetException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryTimesheetException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryTimesheetException item)
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

    public List<PrimaryTimesheetException> List()
    {
        var outValue = new List<PrimaryTimesheetException>();
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
public sealed class CachedPrimaryTimesheetExceptionStore(MemoryPrimaryTimesheetExceptionStore inner, PrimaryTimesheetExceptionCache cache, int hits, int misses) : IPrimaryTimesheetExceptionStore
{
    public MemoryPrimaryTimesheetExceptionStore Inner { get; set; } = inner;
    public PrimaryTimesheetExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryTimesheetException? Find(string code)
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

    public void Save(PrimaryTimesheetException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryTimesheetException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary timesheet exceptions. Empty fields match all.</summary>
public sealed record PrimaryTimesheetExceptionQuery(string Source, int MinVersion, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryTimesheetException item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Version < MinVersion)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryTimesheetException> Apply(List<PrimaryTimesheetException> items)
    {
        var matched = Collections.Filter(items, (PrimaryTimesheetException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary timesheet exception store.</summary>
public static class PrimaryTimesheetExceptionStore
{
    public static IPrimaryTimesheetExceptionStore NewMemoryPrimaryTimesheetExceptionStore()
    {
        return new MemoryPrimaryTimesheetExceptionStore(
            new Dictionary<string, PrimaryTimesheetException>(),
            new List<string>());
    }

    public static IPrimaryTimesheetExceptionStore NewCachedPrimaryTimesheetExceptionStore(MemoryPrimaryTimesheetExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryTimesheetExceptionStore(
            inner,
            PrimaryTimesheetExceptionCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryTimesheetException> FindPrimaryTimesheetExceptions(IPrimaryTimesheetExceptionStore store, PrimaryTimesheetExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
