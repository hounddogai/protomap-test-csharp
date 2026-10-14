namespace Acme.Workforce.Talent.CareerPath.PathDigest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual career path profile.</summary>
public sealed record ManualCareerPathProfileView(string CodeText, string Name, string ChannelText, string Category, string Unit, string PhoneText, string DescriptionText, string Minimum, string Retries, string WeightText, string StatusLabel)
{
    public static ManualCareerPathProfileView ToManualCareerPathProfileView(ManualCareerPathProfile item)
    {
        return new ManualCareerPathProfileView(
            item.Code,
            item.Name,
            item.Channel,
            item.Category,
            Text.Truncate(item.Unit, 40),
            item.Phone,
            Text.Truncate(item.Description, 40),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualCareerPathProfileView> ToManualCareerPathProfileViews(List<ManualCareerPathProfile> items)
    {
        var views = new List<ManualCareerPathProfileView>();
        foreach (var item in items)
        {
            views.Add(ToManualCareerPathProfileView(item));
        }
        return views;
    }

    public static double SumManualCareerPathProfileView(ManualCareerPathProfileView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.WeightText, CultureInfo.InvariantCulture);
        return parsedMinimum + (double)parsedRetries + parsedWeight;
    }
}
