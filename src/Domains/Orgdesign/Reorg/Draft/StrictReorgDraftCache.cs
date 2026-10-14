namespace Acme.Workforce.Orgdesign.Reorg.Draft;

using Acme.Workforce.Kit.Clock;

/// <summary>StrictReorgDraftCache keeps StrictReorgDraft records, as TtlCache keeps any.</summary>
public sealed class StrictReorgDraftCache(Dictionary<string, StrictReorgDraft> entries, Dictionary<string, int> expires, List<string> order, int capacity, int ttl, IClock clock)
{
    public Dictionary<string, StrictReorgDraft> Entries { get; set; } = entries;
    public Dictionary<string, int> Expires { get; set; } = expires;
    public List<string> Order { get; set; } = order;
    public int Capacity { get; set; } = capacity;
    public int Ttl { get; set; } = ttl;
    public IClock Clock { get; set; } = clock;

    public static StrictReorgDraftCache Create(int capacity, int ttl, IClock clock)
    {
        return new StrictReorgDraftCache(
            new Dictionary<string, StrictReorgDraft>(),
            new Dictionary<string, int>(),
            new List<string>(),
            capacity,
            ttl,
            clock);
    }

    public StrictReorgDraft? Get(string key)
    {
        if (!Expires.TryGetValue(key, out var deadline))
        {
            return default;
        }
        if (Clock.Now() > deadline)
        {
            Entries.Remove(key);
            Expires.Remove(key);
            return default;
        }
        if (!Entries.TryGetValue(key, out var valueValue))
        {
            return default;
        }
        return valueValue;
    }

    public void Put(string key, StrictReorgDraft valueValue)
    {
        if (!Entries.ContainsKey(key))
        {
            Order.Add(key);
        }
        Entries[key] = valueValue;
        Expires[key] = Clock.Now() + Ttl;
        while (Order.Count > Capacity && Order.Count > 0)
        {
            var oldest = Order[0];
            Order = Order.Skip(1).ToList();
            Entries.Remove(oldest);
            Expires.Remove(oldest);
        }
    }

    public int Size()
    {
        return Entries.Count;
    }

    public int PurgeExpired()
    {
        var now = Clock.Now();
        var kept = new List<string>();
        var removed = 0;
        foreach (var key in Order)
        {
            var deadline = Expires.GetValueOrDefault(key);
            if (now > deadline)
            {
                Entries.Remove(key);
                Expires.Remove(key);
                removed += 1;
                continue;
            }
            kept.Add(key);
        }
        Order = kept;
        return removed;
    }
}
