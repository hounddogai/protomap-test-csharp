namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps manual statement of work groups by code.</summary>
public interface IManualStatementOfWorkGroupStore
{
    ManualStatementOfWorkGroup? Find(string code);
    void Save(ManualStatementOfWorkGroup item);
    bool Remove(string code);
    List<ManualStatementOfWorkGroup> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryManualStatementOfWorkGroupStore(Dictionary<string, ManualStatementOfWorkGroup> items, List<string> order) : IManualStatementOfWorkGroupStore
{
    public Dictionary<string, ManualStatementOfWorkGroup> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public ManualStatementOfWorkGroup? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(ManualStatementOfWorkGroup item)
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

    public List<ManualStatementOfWorkGroup> List()
    {
        var outValue = new List<ManualStatementOfWorkGroup>();
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
public sealed class CachedManualStatementOfWorkGroupStore(MemoryManualStatementOfWorkGroupStore inner, ManualStatementOfWorkGroupCache cache, int hits, int misses) : IManualStatementOfWorkGroupStore
{
    public MemoryManualStatementOfWorkGroupStore Inner { get; set; } = inner;
    public ManualStatementOfWorkGroupCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public ManualStatementOfWorkGroup? Find(string code)
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

    public void Save(ManualStatementOfWorkGroup item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<ManualStatementOfWorkGroup> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects manual statement of work groups. Empty fields match all.</summary>
public sealed record ManualStatementOfWorkGroupQuery(string Label, double MinAmount, List<string> Statuses, int Limit)
{
    public bool Matches(ManualStatementOfWorkGroup item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Amount < MinAmount)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<ManualStatementOfWorkGroup> Apply(List<ManualStatementOfWorkGroup> items)
    {
        var matched = Collections.Filter(items, (ManualStatementOfWorkGroup candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of manual statement of work group store.</summary>
public static class ManualStatementOfWorkGroupStore
{
    public static IManualStatementOfWorkGroupStore NewMemoryManualStatementOfWorkGroupStore()
    {
        return new MemoryManualStatementOfWorkGroupStore(
            new Dictionary<string, ManualStatementOfWorkGroup>(),
            new List<string>());
    }

    public static IManualStatementOfWorkGroupStore NewCachedManualStatementOfWorkGroupStore(MemoryManualStatementOfWorkGroupStore inner, int ttl, IClock clock)
    {
        return new CachedManualStatementOfWorkGroupStore(
            inner,
            ManualStatementOfWorkGroupCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<ManualStatementOfWorkGroup> FindManualStatementOfWorkGroups(IManualStatementOfWorkGroupStore store, ManualStatementOfWorkGroupQuery query)
    {
        return query.Apply(store.List());
    }
}
