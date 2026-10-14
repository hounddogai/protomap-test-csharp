namespace Acme.Workforce.Contractors.Timesheet.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps regional timesheet reviews by code.</summary>
public interface IRegionalTimesheetReviewStore
{
    RegionalTimesheetReview? Find(string code);
    void Save(RegionalTimesheetReview item);
    bool Remove(string code);
    List<RegionalTimesheetReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRegionalTimesheetReviewStore(Dictionary<string, RegionalTimesheetReview> items, List<string> order) : IRegionalTimesheetReviewStore
{
    public Dictionary<string, RegionalTimesheetReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RegionalTimesheetReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RegionalTimesheetReview item)
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

    public List<RegionalTimesheetReview> List()
    {
        var outValue = new List<RegionalTimesheetReview>();
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
public sealed class CachedRegionalTimesheetReviewStore(MemoryRegionalTimesheetReviewStore inner, RegionalTimesheetReviewCache cache, int hits, int misses) : IRegionalTimesheetReviewStore
{
    public MemoryRegionalTimesheetReviewStore Inner { get; set; } = inner;
    public RegionalTimesheetReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RegionalTimesheetReview? Find(string code)
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

    public void Save(RegionalTimesheetReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RegionalTimesheetReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects regional timesheet reviews. Empty fields match all.</summary>
public sealed record RegionalTimesheetReviewQuery(string Team, int MinAttempts, List<string> Statuses, int Limit)
{
    public bool Matches(RegionalTimesheetReview item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
        {
            return false;
        }
        if (item.Attempts < MinAttempts)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RegionalTimesheetReview> Apply(List<RegionalTimesheetReview> items)
    {
        var matched = Collections.Filter(items, (RegionalTimesheetReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of regional timesheet review store.</summary>
public static class RegionalTimesheetReviewStore
{
    public static IRegionalTimesheetReviewStore NewMemoryRegionalTimesheetReviewStore()
    {
        return new MemoryRegionalTimesheetReviewStore(
            new Dictionary<string, RegionalTimesheetReview>(),
            new List<string>());
    }

    public static IRegionalTimesheetReviewStore NewCachedRegionalTimesheetReviewStore(MemoryRegionalTimesheetReviewStore inner, int ttl, IClock clock)
    {
        return new CachedRegionalTimesheetReviewStore(
            inner,
            RegionalTimesheetReviewCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<RegionalTimesheetReview> FindRegionalTimesheetReviews(IRegionalTimesheetReviewStore store, RegionalTimesheetReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
