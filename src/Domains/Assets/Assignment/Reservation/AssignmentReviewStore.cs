namespace Acme.Workforce.Assets.Assignment.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps assignment reviews by code.</summary>
public interface IAssignmentReviewStore
{
    AssignmentReview? Find(string code);
    void Save(AssignmentReview item);
    bool Remove(string code);
    List<AssignmentReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryAssignmentReviewStore(Dictionary<string, AssignmentReview> items, List<string> order) : IAssignmentReviewStore
{
    public Dictionary<string, AssignmentReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public AssignmentReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(AssignmentReview item)
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

    public List<AssignmentReview> List()
    {
        var outValue = new List<AssignmentReview>();
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
public sealed class CachedAssignmentReviewStore(MemoryAssignmentReviewStore inner, AssignmentReviewCache cache, int hits, int misses) : IAssignmentReviewStore
{
    public MemoryAssignmentReviewStore Inner { get; set; } = inner;
    public AssignmentReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public AssignmentReview? Find(string code)
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

    public void Save(AssignmentReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<AssignmentReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects assignment reviews. Empty fields match all.</summary>
public sealed record AssignmentReviewQuery(string Team, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(AssignmentReview item)
    {
        if (Team != "" && Text.Normalize(item.Team) != Text.Normalize(Team))
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

    public List<AssignmentReview> Apply(List<AssignmentReview> items)
    {
        var matched = Collections.Filter(items, (AssignmentReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of assignment review store.</summary>
public static class AssignmentReviewStore
{
    public static IAssignmentReviewStore NewMemoryAssignmentReviewStore()
    {
        return new MemoryAssignmentReviewStore(new Dictionary<string, AssignmentReview>(), new List<string>());
    }

    public static IAssignmentReviewStore NewCachedAssignmentReviewStore(MemoryAssignmentReviewStore inner, int ttl, IClock clock)
    {
        return new CachedAssignmentReviewStore(inner, AssignmentReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<AssignmentReview> FindAssignmentReviews(IAssignmentReviewStore store, AssignmentReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
