namespace Acme.Workforce.Kit.Tests.Flags;

using Acme.Workforce.Kit.Flags;

/// <summary>Checks of flags code, run by the project's runner.</summary>
public static class FlagsChecks
{
    public static void HonorsAllowLists()
    {
        var registry = Flags.NewRegistry();
        registry.Register(new Flag("beta", true, new AllowListStrategy(new List<string> { "tenant-a" })));
        Check.True(registry.Enabled("beta", "tenant-a"), "tenant-a");
        Check.True(!registry.Enabled("beta", "tenant-b"), "tenant-b");
        Check.True(!registry.Enabled("missing", "tenant-a"), "missing flag");
    }

    public static void RunAll()
    {
        HonorsAllowLists();
    }
}
