namespace Acme.Workforce.Kit.Tests.Table;

using Acme.Workforce.Kit.Table;

/// <summary>Checks of table code, run by the project's runner.</summary>
public static class TableChecks
{
    public static void RejectsRowsOfTheWrongWidth()
    {
        var grid = Table.NewTable("Totals", new List<string> { "region", "total" });
        Check.Fails(() => grid.AddRow(new List<string> { "north" }), "short row");
        try
        {
            grid.AddRow(new List<string> { "north", "12" });
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(grid.Csv(), "region,total\nnorth,12", "csv");
    }

    public static void RunAll()
    {
        RejectsRowsOfTheWrongWidth();
    }
}
