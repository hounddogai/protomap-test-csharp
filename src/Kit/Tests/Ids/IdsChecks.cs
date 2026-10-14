namespace Acme.Workforce.Kit.Tests.Ids;

using Acme.Workforce.Kit.Ids;

/// <summary>Checks of ids code, run by the project's runner.</summary>
public static class IdsChecks
{
    public static void PadsSequenceNumbers()
    {
        Check.Equal(Ids.NewId("inv", 42), "INV-000042", "id");
        Check.True(Ids.IsReference("INV-000042", "inv"), "reference");
    }

    public static void RunAll()
    {
        PadsSequenceNumbers();
    }
}
