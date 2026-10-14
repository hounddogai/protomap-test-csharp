namespace Acme.Workforce.Assets.Warranty.Allocation;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled warranty manifest.</summary>
public sealed record ScheduledWarrantyManifestView(string CodeText, string LabelText, string UpdatedAt, string DescriptionText, string Grade, string Budget, string SalaryBand, string Sequence, string StatusLabel)
{
    public static ScheduledWarrantyManifestView ToScheduledWarrantyManifestView(ScheduledWarrantyManifest item)
    {
        return new ScheduledWarrantyManifestView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Label, 40),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Description, 80),
            item.Grade,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.SalaryBand, 120),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledWarrantyManifestView> ToScheduledWarrantyManifestViews(List<ScheduledWarrantyManifest> items)
    {
        var views = new List<ScheduledWarrantyManifestView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledWarrantyManifestView(item));
        }
        return views;
    }

    public static double SumScheduledWarrantyManifestView(ScheduledWarrantyManifestView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedBudget + (double)parsedSequence;
    }
}
