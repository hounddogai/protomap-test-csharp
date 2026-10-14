namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnReservationEngineChecks
{
    public static void AssetReturnReservationEngineKeepsResultsWithinBounds()
    {
        var engine = AssetReturnReservationEngine.DefaultAssetReturnReservationEngine();
        var item = AssetReturnReservation.NewAssetReturnReservation("code-286", "label-900");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AssetReturnReservationEngineKeepsResultsWithinBounds();
    }
}
