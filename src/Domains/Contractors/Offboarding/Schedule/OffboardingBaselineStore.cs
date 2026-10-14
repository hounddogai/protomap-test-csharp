namespace Acme.Workforce.Contractors.Offboarding.Schedule;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps offboarding baselines by code.</summary>
public interface IOffboardingBaselineStore
{
    OffboardingBaseline? Find(string code);
    void Save(OffboardingBaseline item);
    bool Remove(string code);
    List<OffboardingBaseline> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryOffboardingBaselineStore(Dictionary<string, OffboardingBaseline> items, List<string> order) : IOffboardingBaselineStore
{
    public Dictionary<string, OffboardingBaseline> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public OffboardingBaseline? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(OffboardingBaseline item)
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

    public List<OffboardingBaseline> List()
    {
        var outValue = new List<OffboardingBaseline>();
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
public sealed class CachedOffboardingBaselineStore(MemoryOffboardingBaselineStore inner, OffboardingBaselineCache cache, int hits, int misses) : IOffboardingBaselineStore
{
    public MemoryOffboardingBaselineStore Inner { get; set; } = inner;
    public OffboardingBaselineCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public OffboardingBaseline? Find(string code)
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

    public void Save(OffboardingBaseline item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<OffboardingBaseline> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects offboarding baselines. Empty fields match all.</summary>
public sealed record OffboardingBaselineQuery(string Title, double MinMinimum, List<string> Statuses, int Limit)
{
    public bool Matches(OffboardingBaseline item)
    {
        if (Title != "" && Text.Normalize(item.Title) != Text.Normalize(Title))
        {
            return false;
        }
        if (item.Minimum < MinMinimum)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<OffboardingBaseline> Apply(List<OffboardingBaseline> items)
    {
        var matched = Collections.Filter(items, (OffboardingBaseline candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of offboarding baseline store.</summary>
public static class OffboardingBaselineStore
{
    public static IOffboardingBaselineStore NewMemoryOffboardingBaselineStore()
    {
        return new MemoryOffboardingBaselineStore(new Dictionary<string, OffboardingBaseline>(), new List<string>());
    }

    public static IOffboardingBaselineStore NewCachedOffboardingBaselineStore(MemoryOffboardingBaselineStore inner, int ttl, IClock clock)
    {
        return new CachedOffboardingBaselineStore(inner, OffboardingBaselineCache.Create(64, ttl, clock), 0, 0);
    }

    public static List<OffboardingBaseline> FindOffboardingBaselines(IOffboardingBaselineStore store, OffboardingBaselineQuery query)
    {
        return query.Apply(store.List());
    }
}
