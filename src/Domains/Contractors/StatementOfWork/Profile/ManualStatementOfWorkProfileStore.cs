namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual statement of work profiles by code.</summary>
public interface IManualStatementOfWorkProfileStore
{
    ManualStatementOfWorkProfile? Find(string code);
    void Save(ManualStatementOfWorkProfile item);
    bool Remove(string code);
    List<ManualStatementOfWorkProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualStatementOfWorkProfileStore(Dictionary<string, ManualStatementOfWorkProfile> items, List<string> order) : IManualStatementOfWorkProfileStore
{
    public Dictionary<string, ManualStatementOfWorkProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualStatementOfWorkProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualStatementOfWorkProfile item)
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

    public List<ManualStatementOfWorkProfile> List()
    {
        var outValue = new List<ManualStatementOfWorkProfile>();
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
public sealed class CachedManualStatementOfWorkProfileStore(MemoryManualStatementOfWorkProfileStore inner, ManualStatementOfWorkProfileCache cache, int hits, int misses) : IManualStatementOfWorkProfileStore
{
    public MemoryManualStatementOfWorkProfileStore Inner { get; set; } = inner;
    public ManualStatementOfWorkProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualStatementOfWorkProfile? Find(string code)
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

    public void Save(ManualStatementOfWorkProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualStatementOfWorkProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual statement of work profiles. Empty fields match all.</summary>
public sealed record ManualStatementOfWorkProfileQuery(string Title, int MinPriority, List<string> Statuses, int Limit)
{
    public bool Matches(ManualStatementOfWorkProfile item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Priority < MinPriority)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualStatementOfWorkProfile> Apply(List<ManualStatementOfWorkProfile> items)
    {
        var matched = Collections.Filter(items, (ManualStatementOfWorkProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual statement of work profile store.</summary>
public static class ManualStatementOfWorkProfileStore
{
    public static IManualStatementOfWorkProfileStore NewMemoryManualStatementOfWorkProfileStore()
    {
        return new MemoryManualStatementOfWorkProfileStore(
            new Dictionary<string, ManualStatementOfWorkProfile>(),
            new List<string>());
    }

    public static IManualStatementOfWorkProfileStore NewCachedManualStatementOfWorkProfileStore(MemoryManualStatementOfWorkProfileStore inner, int ttl, IClock clock)
    {
        return new CachedManualStatementOfWorkProfileStore(
            inner,
            ManualStatementOfWorkProfileCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<ManualStatementOfWorkProfile> FindManualStatementOfWorkProfiles(IManualStatementOfWorkProfileStore store, ManualStatementOfWorkProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
