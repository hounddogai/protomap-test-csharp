namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AssetReturnPolicyViewChecks
{
    public static void AssetReturnPolicyViewShowsTheStatusInCapitals()
    {
        var item = AssetReturnPolicy.NewAssetReturnPolicy("code-814", "label-922");
        var view = AssetReturnPolicyView.ToAssetReturnPolicyView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AssetReturnPolicyView.ToAssetReturnPolicyViews(new List<AssetReturnPolicy> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AssetReturnPolicyViewShowsTheStatusInCapitals();
    }
}
