namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global career path profile.</summary>
public sealed record GlobalCareerPathProfileView(string Code, string Title, string Capacity, string NameText, string MaximumText, string ExpiresAt, string Attempts, string JobCodeText, string VisibleText, string StatusLabel)
{
    public static GlobalCareerPathProfileView ToGlobalCareerPathProfileView(GlobalCareerPathProfile item)
    {
        return new GlobalCareerPathProfileView(
            item.Code,
            item.Title,
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.JobCode, 120),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalCareerPathProfileView> ToGlobalCareerPathProfileViews(List<GlobalCareerPathProfile> items)
    {
        var views = new List<GlobalCareerPathProfileView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalCareerPathProfileView(item));
        }
        return views;
    }

    public static double SumGlobalCareerPathProfileView(GlobalCareerPathProfileView view)
    {
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        return (double)parsedCapacity + parsedMaximum + (double)parsedExpiresAt;
    }
}
