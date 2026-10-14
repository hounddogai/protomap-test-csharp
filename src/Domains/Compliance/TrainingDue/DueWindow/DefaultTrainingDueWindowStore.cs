namespace Acme.Workforce.Compliance.TrainingDue.DueWindow;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default training due windows by code.</summary>
public interface IDefaultTrainingDueWindowStore
{
    DefaultTrainingDueWindow? Find(string code);
    void Save(DefaultTrainingDueWindow item);
    bool Remove(string code);
    List<DefaultTrainingDueWindow> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultTrainingDueWindowStore(Dictionary<string, DefaultTrainingDueWindow> items, List<string> order) : IDefaultTrainingDueWindowStore
{
    public Dictionary<string, DefaultTrainingDueWindow> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultTrainingDueWindow? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultTrainingDueWindow item)
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

    public List<DefaultTrainingDueWindow> List()
    {
        var outValue = new List<DefaultTrainingDueWindow>();
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
public sealed class CachedDefaultTrainingDueWindowStore(MemoryDefaultTrainingDueWindowStore inner, DefaultTrainingDueWindowCache cache, int hits, int misses) : IDefaultTrainingDueWindowStore
{
    public MemoryDefaultTrainingDueWindowStore Inner { get; set; } = inner;
    public DefaultTrainingDueWindowCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultTrainingDueWindow? Find(string code)
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

    public void Save(DefaultTrainingDueWindow item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultTrainingDueWindow> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default training due windows. Empty fields match all.</summary>
public sealed record DefaultTrainingDueWindowQuery(string Name, int MinRevision, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultTrainingDueWindow item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.Revision < MinRevision)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultTrainingDueWindow> Apply(List<DefaultTrainingDueWindow> items)
    {
        var matched = Collections.Filter(items, (DefaultTrainingDueWindow candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default training due window store.</summary>
public static class DefaultTrainingDueWindowStore
{
    public static IDefaultTrainingDueWindowStore NewMemoryDefaultTrainingDueWindowStore()
    {
        return new MemoryDefaultTrainingDueWindowStore(
            new Dictionary<string, DefaultTrainingDueWindow>(),
            new List<string>());
    }

    public static IDefaultTrainingDueWindowStore NewCachedDefaultTrainingDueWindowStore(MemoryDefaultTrainingDueWindowStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultTrainingDueWindowStore(
            inner,
            DefaultTrainingDueWindowCache.Create(64, ttl, clock),
            0,
            0);
    }

    public static List<DefaultTrainingDueWindow> FindDefaultTrainingDueWindows(IDefaultTrainingDueWindowStore store, DefaultTrainingDueWindowQuery query)
    {
        return query.Apply(store.List());
    }
}
