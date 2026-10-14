namespace Acme.Workforce.Talent.Pool.Agreement;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps rolling pool exceptions by code.</summary>
public interface IRollingPoolExceptionStore
{
    RollingPoolException? Find(string code);
    void Save(RollingPoolException item);
    bool Remove(string code);
    List<RollingPoolException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryRollingPoolExceptionStore(Dictionary<string, RollingPoolException> items, List<string> order) : IRollingPoolExceptionStore
{
    public Dictionary<string, RollingPoolException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public RollingPoolException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(RollingPoolException item)
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

    public List<RollingPoolException> List()
    {
        var outValue = new List<RollingPoolException>();
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
public sealed class CachedRollingPoolExceptionStore(MemoryRollingPoolExceptionStore inner, RollingPoolExceptionCache cache, int hits, int misses) : IRollingPoolExceptionStore
{
    public MemoryRollingPoolExceptionStore Inner { get; set; } = inner;
    public RollingPoolExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public RollingPoolException? Find(string code)
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

    public void Save(RollingPoolException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<RollingPoolException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects rolling pool exceptions. Empty fields match all.</summary>
public sealed record RollingPoolExceptionQuery(string Label, double MinBudget, List<string> Statuses, int Limit)
{
    public bool Matches(RollingPoolException item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Budget < MinBudget)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<RollingPoolException> Apply(List<RollingPoolException> items)
    {
        var matched = Collections.Filter(items, (RollingPoolException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of rolling pool exception store.</summary>
public static class RollingPoolExceptionStore
{
    public static IRollingPoolExceptionStore NewMemoryRollingPoolExceptionStore()
    {
        return new MemoryRollingPoolExceptionStore(
            new Dictionary<string, RollingPoolException>(),
            new List<string>());
    }

    public static IRollingPoolExceptionStore NewCachedRollingPoolExceptionStore(MemoryRollingPoolExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedRollingPoolExceptionStore(inner, RollingPoolExceptionCache.Create(128, ttl, clock), 0, 0);
    }

    public static List<RollingPoolException> FindRollingPoolExceptions(IRollingPoolExceptionStore store, RollingPoolExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
