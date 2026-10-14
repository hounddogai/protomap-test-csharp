namespace Acme.Workforce.Kit.Tests.Collections;

using Acme.Workforce.Kit.Collections;

/// <summary>Checks of collections code, run by the project's runner.</summary>
public static class CollectionsChecks
{
    public static void ChunksIntoBatches()
    {
        var items = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        var chunks = Collections.Chunk(items, 3);
        Check.Equal(chunks.Count, 3, "chunks");
        Check.Equal(chunks[2].Count, 1, "last chunk");
    }

    public static void FiltersAndCounts()
    {
        var items = new List<int> { 4, 9, 15, 16, 23, 42 };
        var even = Collections.Filter(items, (int n) => n % 2 == 0);
        Check.Equal(even.Count, 3, "even");
        var counts = Collections.CountBy(items, (int n) =>
        {
            if (n > 10)
            {
                return "big";
            }
            return "small";
        });
        Check.Equal(counts.GetValueOrDefault("big"), 4, "big");
    }

    public static void DropsRepeatedStrings()
    {
        Check.Equal(Collections.UniqueStrings(new List<string> { "a", "b", "a" }).Count, 2, "unique");
    }

    public static void RunAll()
    {
        ChunksIntoBatches();
        FiltersAndCounts();
        DropsRepeatedStrings();
    }
}
