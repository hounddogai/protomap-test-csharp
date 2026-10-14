namespace Acme.Workforce.Compensation.Benchmark.Audit;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark notices by code.</summary>
public interface IBenchmarkNoticeStore
{
    BenchmarkNotice? Find(string code);
    void Save(BenchmarkNotice item);
    bool Remove(string code);
    List<BenchmarkNotice> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkNoticeStore(Dictionary<string, BenchmarkNotice> items, List<string> order) : IBenchmarkNoticeStore
{
    public Dictionary<string, BenchmarkNotice> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkNotice? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkNotice item)
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

    public List<BenchmarkNotice> List()
    {
        var outValue = new List<BenchmarkNotice>();
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
public sealed class CachedBenchmarkNoticeStore(MemoryBenchmarkNoticeStore inner, BenchmarkNoticeCache cache, int hits, int misses) : IBenchmarkNoticeStore
{
    public MemoryBenchmarkNoticeStore Inner { get; set; } = inner;
    public BenchmarkNoticeCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkNotice? Find(string code)
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

    public void Save(BenchmarkNotice item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkNotice> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark notices. Empty fields match all.</summary>
public sealed record BenchmarkNoticeQuery(string Channel, int MinCreatedAt, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkNotice item)
    {
        if (Channel != "" && Text.Normalize(item.Channel) != Text.Normalize(Channel))
        {
            return false;
        }
        if (item.CreatedAt < MinCreatedAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<BenchmarkNotice> Apply(List<BenchmarkNotice> items)
    {
        var matched = Collections.Filter(items, (BenchmarkNotice candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark notice store.</summary>
public static class BenchmarkNoticeStore
{
    public static IBenchmarkNoticeStore NewMemoryBenchmarkNoticeStore()
    {
        return new MemoryBenchmarkNoticeStore(new Dictionary<string, BenchmarkNotice>(), new List<string>());
    }

    public static IBenchmarkNoticeStore NewCachedBenchmarkNoticeStore(MemoryBenchmarkNoticeStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkNoticeStore(inner, BenchmarkNoticeCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<BenchmarkNotice> FindBenchmarkNotices(IBenchmarkNoticeStore store, BenchmarkNoticeQuery query)
    {
        return query.Apply(store.List());
    }
}
