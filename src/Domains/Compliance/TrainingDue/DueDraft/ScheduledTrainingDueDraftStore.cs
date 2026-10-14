namespace Acme.Workforce.Compliance.TrainingDue.DueDraft;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps scheduled training due drafts by code.</summary>
public interface IScheduledTrainingDueDraftStore
{
    ScheduledTrainingDueDraft? Find(string code);
    void Save(ScheduledTrainingDueDraft item);
    bool Remove(string code);
    List<ScheduledTrainingDueDraft> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryScheduledTrainingDueDraftStore(Dictionary<string, ScheduledTrainingDueDraft> items, List<string> order) : IScheduledTrainingDueDraftStore
{
    public Dictionary<string, ScheduledTrainingDueDraft> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ScheduledTrainingDueDraft? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ScheduledTrainingDueDraft item)
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

    public List<ScheduledTrainingDueDraft> List()
    {
        var outValue = new List<ScheduledTrainingDueDraft>();
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
public sealed class CachedScheduledTrainingDueDraftStore(MemoryScheduledTrainingDueDraftStore inner, ScheduledTrainingDueDraftCache cache, int hits, int misses) : IScheduledTrainingDueDraftStore
{
    public MemoryScheduledTrainingDueDraftStore Inner { get; set; } = inner;
    public ScheduledTrainingDueDraftCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ScheduledTrainingDueDraft? Find(string code)
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

    public void Save(ScheduledTrainingDueDraft item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ScheduledTrainingDueDraft> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects scheduled training due drafts. Empty fields match all.</summary>
public sealed record ScheduledTrainingDueDraftQuery(string Name, int MinLimit, List<string> Statuses, int Limit)
{
    public bool Matches(ScheduledTrainingDueDraft item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Limit < MinLimit)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ScheduledTrainingDueDraft> Apply(List<ScheduledTrainingDueDraft> items)
    {
        var matched = Collections.Filter(items, (ScheduledTrainingDueDraft candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of scheduled training due draft store.</summary>
public static class ScheduledTrainingDueDraftStore
{
    public static IScheduledTrainingDueDraftStore NewMemoryScheduledTrainingDueDraftStore()
    {
        return new MemoryScheduledTrainingDueDraftStore(
            new Dictionary<string, ScheduledTrainingDueDraft>(),
            new List<string>());
    }

    public static IScheduledTrainingDueDraftStore NewCachedScheduledTrainingDueDraftStore(MemoryScheduledTrainingDueDraftStore inner, int ttl, IClock clock)
    {
        return new CachedScheduledTrainingDueDraftStore(
            inner,
            ScheduledTrainingDueDraftCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ScheduledTrainingDueDraft> FindScheduledTrainingDueDrafts(IScheduledTrainingDueDraftStore store, ScheduledTrainingDueDraftQuery query)
    {
        return query.Apply(store.List());
    }
}
