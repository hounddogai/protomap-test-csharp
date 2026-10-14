namespace Acme.Workforce.Talent.CareerPath.Digest;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps career path checkpoints by code.</summary>
public interface ICareerPathCheckpointStore
{
    CareerPathCheckpoint? Find(string code);
    void Save(CareerPathCheckpoint item);
    bool Remove(string code);
    List<CareerPathCheckpoint> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryCareerPathCheckpointStore(Dictionary<string, CareerPathCheckpoint> items, List<string> order) : ICareerPathCheckpointStore
{
    public Dictionary<string, CareerPathCheckpoint> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public CareerPathCheckpoint? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(CareerPathCheckpoint item)
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

    public List<CareerPathCheckpoint> List()
    {
        var outValue = new List<CareerPathCheckpoint>();
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
public sealed class CachedCareerPathCheckpointStore(MemoryCareerPathCheckpointStore inner, CareerPathCheckpointCache cache, int hits, int misses) : ICareerPathCheckpointStore
{
    public MemoryCareerPathCheckpointStore Inner { get; set; } = inner;
    public CareerPathCheckpointCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public CareerPathCheckpoint? Find(string code)
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

    public void Save(CareerPathCheckpoint item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<CareerPathCheckpoint> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects career path checkpoints. Empty fields match all.</summary>
public sealed record CareerPathCheckpointQuery(string Label, double MinRatio, List<string> Statuses, int Limit)
{
    public bool Matches(CareerPathCheckpoint item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<CareerPathCheckpoint> Apply(List<CareerPathCheckpoint> items)
    {
        var matched = Collections.Filter(items, (CareerPathCheckpoint candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of career path checkpoint store.</summary>
public static class CareerPathCheckpointStore
{
    public static ICareerPathCheckpointStore NewMemoryCareerPathCheckpointStore()
    {
        return new MemoryCareerPathCheckpointStore(
            new Dictionary<string, CareerPathCheckpoint>(),
            new List<string>());
    }

    public static ICareerPathCheckpointStore NewCachedCareerPathCheckpointStore(MemoryCareerPathCheckpointStore inner, int ttl, IClock clock)
    {
        return new CachedCareerPathCheckpointStore(inner, CareerPathCheckpointCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<CareerPathCheckpoint> FindCareerPathCheckpoints(ICareerPathCheckpointStore store, CareerPathCheckpointQuery query)
    {
        return query.Apply(store.List());
    }
}
