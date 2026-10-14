namespace Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps legacy exception profiles by code.</summary>
public interface ILegacyExceptionProfileStore
{
    LegacyExceptionProfile? Find(string code);
    void Save(LegacyExceptionProfile item);
    bool Remove(string code);
    List<LegacyExceptionProfile> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryLegacyExceptionProfileStore(Dictionary<string, LegacyExceptionProfile> items, List<string> order) : ILegacyExceptionProfileStore
{
    public Dictionary<string, LegacyExceptionProfile> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public LegacyExceptionProfile? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(LegacyExceptionProfile item)
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

    public List<LegacyExceptionProfile> List()
    {
        var outValue = new List<LegacyExceptionProfile>();
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
public sealed class CachedLegacyExceptionProfileStore(MemoryLegacyExceptionProfileStore inner, LegacyExceptionProfileCache cache, int hits, int misses) : ILegacyExceptionProfileStore
{
    public MemoryLegacyExceptionProfileStore Inner { get; set; } = inner;
    public LegacyExceptionProfileCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public LegacyExceptionProfile? Find(string code)
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

    public void Save(LegacyExceptionProfile item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<LegacyExceptionProfile> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects legacy exception profiles. Empty fields match all.</summary>
public sealed record LegacyExceptionProfileQuery(string Label, int MinEndsAt, List<string> Statuses, int Limit)
{
    public bool Matches(LegacyExceptionProfile item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
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

    public List<LegacyExceptionProfile> Apply(List<LegacyExceptionProfile> items)
    {
        var matched = Collections.Filter(items, (LegacyExceptionProfile candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of legacy exception profile store.</summary>
public static class LegacyExceptionProfileStore
{
    public static ILegacyExceptionProfileStore NewMemoryLegacyExceptionProfileStore()
    {
        return new MemoryLegacyExceptionProfileStore(
            new Dictionary<string, LegacyExceptionProfile>(),
            new List<string>());
    }

    public static ILegacyExceptionProfileStore NewCachedLegacyExceptionProfileStore(MemoryLegacyExceptionProfileStore inner, int ttl, IClock clock)
    {
        return new CachedLegacyExceptionProfileStore(
            inner,
            LegacyExceptionProfileCache.Create(256, ttl, clock),
            0,
            0);
    }

    public static List<LegacyExceptionProfile> FindLegacyExceptionProfiles(ILegacyExceptionProfileStore store, LegacyExceptionProfileQuery query)
    {
        return query.Apply(store.List());
    }
}
