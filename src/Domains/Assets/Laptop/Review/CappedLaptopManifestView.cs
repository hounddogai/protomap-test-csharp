namespace Acme.Workforce.Assets.Laptop.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped laptop manifest.</summary>
public sealed record CappedLaptopManifestView(string Code, string NameText, string SequenceText, string Approved, string UpdatedAt, string DurationMinutesText, string Reviewer, string OwnerIdText, string StatusLabel)
{
    public static CappedLaptopManifestView ToCappedLaptopManifestView(CappedLaptopManifest item)
    {
        return new CappedLaptopManifestView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Reviewer, 80),
            item.OwnerId,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedLaptopManifestView> ToCappedLaptopManifestViews(List<CappedLaptopManifest> items)
    {
        var views = new List<CappedLaptopManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedLaptopManifestView(item));
        }
        return views;
    }

    public static double SumCappedLaptopManifestView(CappedLaptopManifestView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedUpdatedAt + (double)parsedDurationMinutes;
    }
}
