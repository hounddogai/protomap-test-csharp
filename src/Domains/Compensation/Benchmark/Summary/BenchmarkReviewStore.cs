namespace Acme.Workforce.Compensation.Benchmark.Summary;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark reviews by code.</summary>
public interface IBenchmarkReviewStore
{
    BenchmarkReview? Find(string code);
    void Save(BenchmarkReview item);
    bool Remove(string code);
    List<BenchmarkReview> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkReviewStore(Dictionary<string, BenchmarkReview> items, List<string> order) : IBenchmarkReviewStore
{
    public Dictionary<string, BenchmarkReview> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkReview? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkReview item)
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

    public List<BenchmarkReview> List()
    {
        var outValue = new List<BenchmarkReview>();
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
public sealed class CachedBenchmarkReviewStore(MemoryBenchmarkReviewStore inner, BenchmarkReviewCache cache, int hits, int misses) : IBenchmarkReviewStore
{
    public MemoryBenchmarkReviewStore Inner { get; set; } = inner;
    public BenchmarkReviewCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkReview? Find(string code)
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

    public void Save(BenchmarkReview item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkReview> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark reviews. Empty fields match all.</summary>
public sealed record BenchmarkReviewQuery(string Department, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkReview item)
    {
        if (Department != "" && Text.Normalize(item.Department) != Text.Normalize(Department))
        {
            return false;
        }
        if (item.WindowDays < MinWindowDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkReview> Apply(List<BenchmarkReview> items)
    {
        var matched = Collections.Filter(items, (BenchmarkReview candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark review store.</summary>
public static class BenchmarkReviewStore
{
    public static IBenchmarkReviewStore NewMemoryBenchmarkReviewStore()
    {
        return new MemoryBenchmarkReviewStore(new Dictionary<string, BenchmarkReview>(), new List<string>());
    }

    public static IBenchmarkReviewStore NewCachedBenchmarkReviewStore(MemoryBenchmarkReviewStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkReviewStore(inner, BenchmarkReviewCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BenchmarkReview> FindBenchmarkReviews(IBenchmarkReviewStore store, BenchmarkReviewQuery query)
    {
        return query.Apply(store.List());
    }
}
