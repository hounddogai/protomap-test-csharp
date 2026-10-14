namespace Acme.Workforce.Engagement.Recognition.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary recognition policy.</summary>
public sealed record PrimaryRecognitionPolicyView(string Code, string LabelText, string Version, string BatchSizeText, string Retries, string Source, string OwnerId, string Location, string StatusLabel)
{
    public static PrimaryRecognitionPolicyView ToPrimaryRecognitionPolicyView(PrimaryRecognitionPolicy item)
    {
        return new PrimaryRecognitionPolicyView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Source, 80),
            Text.Truncate(item.OwnerId, 120),
            item.Location,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryRecognitionPolicyView> ToPrimaryRecognitionPolicyViews(List<PrimaryRecognitionPolicy> items)
    {
        var views = new List<PrimaryRecognitionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryRecognitionPolicyView(item));
        }
        return views;
    }

    public static double SumPrimaryRecognitionPolicyView(PrimaryRecognitionPolicyView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedBatchSize + (double)parsedRetries;
    }
}
