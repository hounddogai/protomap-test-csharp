namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps training due agreements by code.</summary>
public interface ITrainingDueAgreementStore
{
    TrainingDueAgreement? Find(string code);
    void Save(TrainingDueAgreement item);
    bool Remove(string code);
    List<TrainingDueAgreement> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryTrainingDueAgreementStore(Dictionary<string, TrainingDueAgreement> items, List<string> order) : ITrainingDueAgreementStore
{
    public Dictionary<string, TrainingDueAgreement> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public TrainingDueAgreement? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(TrainingDueAgreement item)
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

    public List<TrainingDueAgreement> List()
    {
        var outValue = new List<TrainingDueAgreement>();
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
public sealed class CachedTrainingDueAgreementStore(MemoryTrainingDueAgreementStore inner, TrainingDueAgreementCache cache, int hits, int misses) : ITrainingDueAgreementStore
{
    public MemoryTrainingDueAgreementStore Inner { get; set; } = inner;
    public TrainingDueAgreementCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public TrainingDueAgreement? Find(string code)
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

    public void Save(TrainingDueAgreement item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<TrainingDueAgreement> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects training due agreements. Empty fields match all.</summary>
public sealed record TrainingDueAgreementQuery(string Source, double MinScore, List<string> Statuses, int Limit)
{
    public bool Matches(TrainingDueAgreement item)
    {
        if (Source != "" && Text.Normalize(item.Source) != Text.Normalize(Source))
        {
            return false;
        }
        if (item.Score < MinScore)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<TrainingDueAgreement> Apply(List<TrainingDueAgreement> items)
    {
        var matched = Collections.Filter(items, (TrainingDueAgreement candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of training due agreement store.</summary>
public static class TrainingDueAgreementStore
{
    public static ITrainingDueAgreementStore NewMemoryTrainingDueAgreementStore()
    {
        return new MemoryTrainingDueAgreementStore(
            new Dictionary<string, TrainingDueAgreement>(),
            new List<string>());
    }

    public static ITrainingDueAgreementStore NewCachedTrainingDueAgreementStore(MemoryTrainingDueAgreementStore inner, int ttl, IClock clock)
    {
        return new CachedTrainingDueAgreementStore(inner, TrainingDueAgreementCache.Create(512, ttl, clock), 0, 0);
    }

    public static List<TrainingDueAgreement> FindTrainingDueAgreements(ITrainingDueAgreementStore store, TrainingDueAgreementQuery query)
    {
        return query.Apply(store.List());
    }
}
