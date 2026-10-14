namespace Acme.Workforce.Contractors.Tests.Vendor.Budget;

using Acme.Workforce.Contractors.Vendor.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class VendorBudgetEngineChecks
{
    public static void VendorBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = VendorBudgetEngine.DefaultVendorBudgetEngine();
        var item = VendorBudget.NewVendorBudget("code-575", "name-607");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorBudgetEngineKeepsResultsWithinBounds();
    }
}
