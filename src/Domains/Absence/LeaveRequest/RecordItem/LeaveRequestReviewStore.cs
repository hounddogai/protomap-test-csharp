namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps leave request reviews by code.</summary>
public interface ILeaveRequestReviewStore
{
    LeaveRequestReview? Find(string code);
    void Save(LeaveRequestReview item);
    bool Remove(string code);
    List<LeaveRequestReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLeaveRequestReviewStore(Dictionary<string, LeaveRequestReview> items, List<string> order) : ILeaveRequestReviewStore
{
    public Dictionary<string, LeaveRequestReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LeaveRequestReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LeaveRequestReview item)
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

    public List<LeaveRequestReview> List()
    {
        var outValue = new List<LeaveRequestReview>();
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
public sealed class CachedLeaveRequestReviewStore(MemoryLeaveRequestReviewStore inner, LeaveRequestReviewCache cache, int hits, int misses) : ILeaveRequestReviewStore
{
    public MemoryLeaveRequestReviewStore Inner { get; set; } = inner;
    public LeaveRequestReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LeaveRequestReview? Find(string code)
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

    public void Save(LeaveRequestReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LeaveRequestReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects leave request reviews. Empty fields match all.</summary>
public sealed record LeaveRequestReviewQuery(string Channel, double MinFte, List<string> Statuses, int Limit)
{
    public bool Matches(LeaveRequestReview item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.Fte < MinFte)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<LeaveRequestReview> Apply(List<LeaveRequestReview> items)
    {
        var matched = Collections.Filter(items, (LeaveRequestReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of leave request review store.</summary>
public static class LeaveRequestReviewStore
{
    public static ILeaveRequestReviewStore NewMemoryLeaveRequestReviewStore()
    {
        return new MemoryLeaveRequestReviewStore(new Dictionary<string, LeaveRequestReview>(), new List<string>());
    }

    public static ILeaveRequestReviewStore NewCachedLeaveRequestReviewStore(MemoryLeaveRequestReviewStore inner, int ttl, IClock clock)
    {
        return new CachedLeaveRequestReviewStore(inner, LeaveRequestReviewCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<LeaveRequestReview> FindLeaveRequestReviews(ILeaveRequestReviewStore store, LeaveRequestReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
