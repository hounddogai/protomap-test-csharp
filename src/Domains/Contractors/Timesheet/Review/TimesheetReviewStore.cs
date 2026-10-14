namespace Acme.Workforce.Contractors.Timesheet.Review;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps timesheet reviews by code.</summary>
public interface ITimesheetReviewStore
{
    TimesheetReview? Find(string code);
    void Save(TimesheetReview item);
    bool Remove(string code);
    List<TimesheetReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTimesheetReviewStore(Dictionary<string, TimesheetReview> items, List<string> order) : ITimesheetReviewStore
{
    public Dictionary<string, TimesheetReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TimesheetReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TimesheetReview item)
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

    public List<TimesheetReview> List()
    {
        var outValue = new List<TimesheetReview>();
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
public sealed class CachedTimesheetReviewStore(MemoryTimesheetReviewStore inner, TimesheetReviewCache cache, int hits, int misses) : ITimesheetReviewStore
{
    public MemoryTimesheetReviewStore Inner { get; set; } = inner;
    public TimesheetReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TimesheetReview? Find(string code)
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

    public void Save(TimesheetReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TimesheetReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects timesheet reviews. Empty fields match all.</summary>
public sealed record TimesheetReviewQuery(string Locale, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(TimesheetReview item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.Ratio < MinRatio)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TimesheetReview> Apply(List<TimesheetReview> items)
    {
        var matched = Collections.Filter(items, (TimesheetReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of timesheet review store.</summary>
public static class TimesheetReviewStore
{
    public static ITimesheetReviewStore NewMemoryTimesheetReviewStore()
    {
        return new MemoryTimesheetReviewStore(new Dictionary<string, TimesheetReview>(), new List<string>());
    }

    public static ITimesheetReviewStore NewCachedTimesheetReviewStore(MemoryTimesheetReviewStore inner, int ttl, IClock clock)
    {
        return new CachedTimesheetReviewStore(inner, TimesheetReviewCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TimesheetReview> FindTimesheetReviews(ITimesheetReviewStore store, TimesheetReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
