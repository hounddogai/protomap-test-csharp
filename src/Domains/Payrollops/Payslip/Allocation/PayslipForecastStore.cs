namespace Acme.Workforce.Payrollops.Payslip.Allocation;

using Acme.Workforce.Kit.Clock;
using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Keeps payslip forecasts by code.</summary>
public interface IPayslipForecastStore
{
    PayslipForecast? Find(string code);
    void Save(PayslipForecast item);
    bool Remove(string code);
    List<PayslipForecast> List();
}

/// <summary>Keeps records in memory, in the order they were first saved.</summary>
public sealed class MemoryPayslipForecastStore(Dictionary<string, PayslipForecast> items, List<string> order) : IPayslipForecastStore
{
    public Dictionary<string, PayslipForecast> Items { get; set; } = items;
    public List<string> Order { get; set; } = order;

    public PayslipForecast? Find(string code)
    {
        if (!Items.TryGetValue(code, out var found))
        {
            return null;
        }
        return found;
    }

    public void Save(PayslipForecast item)
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

    public List<PayslipForecast> List()
    {
        var outValue = new List<PayslipForecast>();
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
public sealed class CachedPayslipForecastStore(MemoryPayslipForecastStore inner, PayslipForecastCache cache, int hits, int misses) : IPayslipForecastStore
{
    public MemoryPayslipForecastStore Inner { get; set; } = inner;
    public PayslipForecastCache Cache { get; set; } = cache;
    public int Hits { get; set; } = hits;
    public int Misses { get; set; } = misses;

    public PayslipForecast? Find(string code)
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

    public void Save(PayslipForecast item)
    {
        Inner.Save(item);
        Cache.Put(item.Code, item);
    }

    public bool Remove(string code)
    {
        // A cached copy expires on its own; reads after a removal may still see it until then.
        return Inner.Remove(code);
    }

    public List<PayslipForecast> List()
    {
        return Inner.List();
    }
}

/// <summary>Selects payslip forecasts. Empty fields match all.</summary>
public sealed record PayslipForecastQuery(string Label, int MinCapacity, List<string> Statuses, int Limit)
{
    public bool Matches(PayslipForecast item)
    {
        if (Label != "" && Text.Normalize(item.Label) != Text.Normalize(Label))
        {
            return false;
        }
        if (item.Capacity < MinCapacity)
        {
            return false;
        }
        if (Statuses.Count > 0 && !Statuses.Contains($"{item.Status}"))
        {
            return false;
        }
        return true;
    }

    public List<PayslipForecast> Apply(List<PayslipForecast> items)
    {
        var matched = Collections.Filter(items, (PayslipForecast candidate) => Matches(candidate));
        if (Limit > 0 && matched.Count > Limit)
        {
            return Collections.Take(matched, Limit);
        }
        return matched;
    }
}

/// <summary>Functions of payslip forecast store.</summary>
public static class PayslipForecastStore
{
    public static IPayslipForecastStore NewMemoryPayslipForecastStore()
    {
        return new MemoryPayslipForecastStore(new Dictionary<string, PayslipForecast>(), new List<string>());
    }

    public static IPayslipForecastStore NewCachedPayslipForecastStore(MemoryPayslipForecastStore inner, int ttl, IClock clock)
    {
        return new CachedPayslipForecastStore(inner, PayslipForecastCache.Create(1024, ttl, clock), 0, 0);
    }

    public static List<PayslipForecast> FindPayslipForecasts(IPayslipForecastStore store, PayslipForecastQuery query)
    {
        return query.Apply(store.List());
    }
}
