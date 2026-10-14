namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps primary training due plans by code.</summary>
public interface IPrimaryTrainingDuePlanStore
{
    PrimaryTrainingDuePlan? Find(string code);
    void Save(PrimaryTrainingDuePlan item);
    bool Remove(string code);
    List<PrimaryTrainingDuePlan> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPrimaryTrainingDuePlanStore(Dictionary<string, PrimaryTrainingDuePlan> items, List<string> order) : IPrimaryTrainingDuePlanStore
{
    public Dictionary<string, PrimaryTrainingDuePlan> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PrimaryTrainingDuePlan? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PrimaryTrainingDuePlan item)
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

    public List<PrimaryTrainingDuePlan> List()
    {
        var outValue = new List<PrimaryTrainingDuePlan>();
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
public sealed class CachedPrimaryTrainingDuePlanStore(MemoryPrimaryTrainingDuePlanStore inner, PrimaryTrainingDuePlanCache cache, int hits, int misses) : IPrimaryTrainingDuePlanStore
{
    public MemoryPrimaryTrainingDuePlanStore Inner { get; set; } = inner;
    public PrimaryTrainingDuePlanCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PrimaryTrainingDuePlan? Find(string code)
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

    public void Save(PrimaryTrainingDuePlan item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PrimaryTrainingDuePlan> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects primary training due plans. Empty fields match all.</summary>
public sealed record PrimaryTrainingDuePlanQuery(string Title, int MinTenureMonths, List<string> Statuses, int Limit)
{
    public bool Matches(PrimaryTrainingDuePlan item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.TenureMonths < MinTenureMonths)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PrimaryTrainingDuePlan> Apply(List<PrimaryTrainingDuePlan> items)
    {
        var matched = Collections.Filter(items, (PrimaryTrainingDuePlan candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of primary training due plan store.</summary>
public static class PrimaryTrainingDuePlanStore
{
    public static IPrimaryTrainingDuePlanStore NewMemoryPrimaryTrainingDuePlanStore()
    {
        return new MemoryPrimaryTrainingDuePlanStore(
            new Dictionary<string, PrimaryTrainingDuePlan>(),
            new List<string>());
    }

    public static IPrimaryTrainingDuePlanStore NewCachedPrimaryTrainingDuePlanStore(MemoryPrimaryTrainingDuePlanStore inner, int ttl, IClock clock)
    {
        return new CachedPrimaryTrainingDuePlanStore(
            inner,
            PrimaryTrainingDuePlanCache.Create(512, ttl, clock),
            0,
            0);
    }

    public static List<PrimaryTrainingDuePlan> FindPrimaryTrainingDuePlans(IPrimaryTrainingDuePlanStore store, PrimaryTrainingDuePlanQuery query)
    {
        return query.Apply(store.List());
    }
}
