namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class BatchedLicenseSeatProfileViewChecks
{
    public static void BatchedLicenseSeatProfileViewShowsTheStatusInCapitals()
    {
        var item = BatchedLicenseSeatProfile.NewBatchedLicenseSeatProfile("code-88", "name-597");
        var view = BatchedLicenseSeatProfileView.ToBatchedLicenseSeatProfileView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedLicenseSeatProfileView.ToBatchedLicenseSeatProfileViews(new List<BatchedLicenseSeatProfile> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedLicenseSeatProfileViewShowsTheStatusInCapitals();
    }
}
