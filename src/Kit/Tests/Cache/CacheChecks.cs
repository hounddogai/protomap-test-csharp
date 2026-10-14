namespace Acme.Workforce.Kit.Tests.Cache;

using Acme.Workforce.Kit.Cache;
using Acme.Workforce.Kit.Testing;

/// <summary>Checks of cache code, run by the project's runner.</summary>
public static class CacheChecks
{
    public static void EvictsTheOldestEntry()
    {
        var cache = Cache.NewTtlCache<string>(2, 60, new FixedClock(1000));
        cache.Put("a", "1");
        cache.Put("b", "2");
        cache.Put("c", "3");
        Check.Equal(cache.Size(), 2, "size");
    }

    public static void ForgetsExpiredEntries()
    {
        var clock = new StepClock(0, 100);
        var cache = Cache.NewTtlCache<string>(8, 150, clock);
        cache.Put("key", "value");
        Check.Equal(cache.PurgeExpired(), 0, "purged early");
        Check.Equal(cache.PurgeExpired(), 1, "purged late");
    }

    public static void RunAll()
    {
        EvictsTheOldestEntry();
        ForgetsExpiredEntries();
    }
}
