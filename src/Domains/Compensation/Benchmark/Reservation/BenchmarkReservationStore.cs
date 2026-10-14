namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps benchmark reservations by code.</summary>
public interface IBenchmarkReservationStore
{
    BenchmarkReservation? Find(string code);
    void Save(BenchmarkReservation item);
    bool Remove(string code);
    List<BenchmarkReservation> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryBenchmarkReservationStore(Dictionary<string, BenchmarkReservation> items, List<string> order) : IBenchmarkReservationStore
{
    public Dictionary<string, BenchmarkReservation> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public BenchmarkReservation? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(BenchmarkReservation item)
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

    public List<BenchmarkReservation> List()
    {
        var outValue = new List<BenchmarkReservation>();
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
public sealed class CachedBenchmarkReservationStore(MemoryBenchmarkReservationStore inner, BenchmarkReservationCache cache, int hits, int misses) : IBenchmarkReservationStore
{
    public MemoryBenchmarkReservationStore Inner { get; set; } = inner;
    public BenchmarkReservationCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public BenchmarkReservation? Find(string code)
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

    public void Save(BenchmarkReservation item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<BenchmarkReservation> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects benchmark reservations. Empty fields match all.</summary>
public sealed record BenchmarkReservationQuery(string Title, int MinWindowDays, List<string> Statuses, int Limit)
{
    public bool Matches(BenchmarkReservation item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
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

    public List<BenchmarkReservation> Apply(List<BenchmarkReservation> items)
    {
        var matched = Collections.Filter(items, (BenchmarkReservation candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of benchmark reservation store.</summary>
public static class BenchmarkReservationStore
{
    public static IBenchmarkReservationStore NewMemoryBenchmarkReservationStore()
    {
        return new MemoryBenchmarkReservationStore(
            new Dictionary<string, BenchmarkReservation>(),
            new List<string>());
    }

    public static IBenchmarkReservationStore NewCachedBenchmarkReservationStore(MemoryBenchmarkReservationStore inner, int ttl, IClock clock)
    {
        return new CachedBenchmarkReservationStore(inner, BenchmarkReservationCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<BenchmarkReservation> FindBenchmarkReservations(IBenchmarkReservationStore store, BenchmarkReservationQuery query)
    {
        return query.Apply(store.List());
    }
}
