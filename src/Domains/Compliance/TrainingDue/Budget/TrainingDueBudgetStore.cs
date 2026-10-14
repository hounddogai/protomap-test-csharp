namespace Acme.Workforce.Compliance.TrainingDue.Budget;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due budgets by code.</summary>
public interface ITrainingDueBudgetStore
{
    TrainingDueBudget? Find(string code);
    void Save(TrainingDueBudget item);
    bool Remove(string code);
    List<TrainingDueBudget> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueBudgetStore(Dictionary<string, TrainingDueBudget> items, List<string> order) : ITrainingDueBudgetStore
{
    public Dictionary<string, TrainingDueBudget> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueBudget? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueBudget item)
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

    public List<TrainingDueBudget> List()
    {
        var outValue = new List<TrainingDueBudget>();
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
public sealed class CachedTrainingDueBudgetStore(MemoryTrainingDueBudgetStore inner, TrainingDueBudgetCache cache, int hits, int misses) : ITrainingDueBudgetStore
{
    public MemoryTrainingDueBudgetStore Inner { get; set; } = inner;
    public TrainingDueBudgetCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueBudget? Find(string code)
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

    public void Save(TrainingDueBudget item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueBudget> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due budgets. Empty fields match all.</summary>
public sealed record TrainingDueBudgetQuery(string Locale, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueBudget item)
    {
        if (Locale != "" && Text.Normalize(item.Locale) != Text.Normalize(Locale))
        {
            return false;
        }
        if (item.EndsAt < MinEndsAt)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueBudget> Apply(List<TrainingDueBudget> items)
    {
        var matched = Collections.Filter(items, (TrainingDueBudget candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due budget store.</summary>
public static class TrainingDueBudgetStore
{
    public static ITrainingDueBudgetStore NewMemoryTrainingDueBudgetStore()
    {
        return new MemoryTrainingDueBudgetStore(new Dictionary<string, TrainingDueBudget>(), new List<string>());
    }

    public static ITrainingDueBudgetStore NewCachedTrainingDueBudgetStore(MemoryTrainingDueBudgetStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueBudgetStore(inner, TrainingDueBudgetCache.Create(256, ttl, clock), 0, 0);
    }

    public static List<TrainingDueBudget> FindTrainingDueBudgets(ITrainingDueBudgetStore store, TrainingDueBudgetQuery query)
    {
        return query.Apply(store.List());
    }
}
