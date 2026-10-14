namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps default policy ack exceptions by code.</summary>
public interface IDefaultPolicyAckExceptionStore
{
    DefaultPolicyAckException? Find(string code);
    void Save(DefaultPolicyAckException item);
    bool Remove(string code);
    List<DefaultPolicyAckException> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryDefaultPolicyAckExceptionStore(Dictionary<string, DefaultPolicyAckException> items, List<string> order) : IDefaultPolicyAckExceptionStore
{
    public Dictionary<string, DefaultPolicyAckException> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public DefaultPolicyAckException? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(DefaultPolicyAckException item)
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

    public List<DefaultPolicyAckException> List()
    {
        var outValue = new List<DefaultPolicyAckException>();
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
public sealed class CachedDefaultPolicyAckExceptionStore(MemoryDefaultPolicyAckExceptionStore inner, DefaultPolicyAckExceptionCache cache, int hits, int misses) : IDefaultPolicyAckExceptionStore
{
    public MemoryDefaultPolicyAckExceptionStore Inner { get; set; } = inner;
    public DefaultPolicyAckExceptionCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public DefaultPolicyAckException? Find(string code)
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

    public void Save(DefaultPolicyAckException item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<DefaultPolicyAckException> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects default policy ack exceptions. Empty fields match all.</summary>
public sealed record DefaultPolicyAckExceptionQuery(string Name, int MinGraceDays, List<string> Statuses, int Limit)
{
    public bool Matches(DefaultPolicyAckException item)
    {
        if (Name != "" && Text.Normalize(item.Name) != Text.Normalize(Name))
        {
            return false;
        }
        if (item.GraceDays < MinGraceDays)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<DefaultPolicyAckException> Apply(List<DefaultPolicyAckException> items)
    {
        var matched = Collections.Filter(items, (DefaultPolicyAckException candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of default policy ack exception store.</summary>
public static class DefaultPolicyAckExceptionStore
{
    public static IDefaultPolicyAckExceptionStore NewMemoryDefaultPolicyAckExceptionStore()
    {
        return new MemoryDefaultPolicyAckExceptionStore(
            new Dictionary<string, DefaultPolicyAckException>(),
            new List<string>());
    }

    public static IDefaultPolicyAckExceptionStore NewCachedDefaultPolicyAckExceptionStore(MemoryDefaultPolicyAckExceptionStore inner, int ttl, IClock clock)
    {
        return new CachedDefaultPolicyAckExceptionStore(
            inner,
            DefaultPolicyAckExceptionCache.Create(1024, ttl, clock),
            0,
            0);
    }

    public static List<DefaultPolicyAckException> FindDefaultPolicyAckExceptions(IDefaultPolicyAckExceptionStore store, DefaultPolicyAckExceptionQuery query)
    {
        return query.Apply(store.List());
    }
}
