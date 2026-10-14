namespace Acme.Workforce.Contractors.Offboarding.Request;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a offboarding bundle.</summary>
public sealed record OffboardingBundleView(string CodeText, string Name, string RateText, string Timezone, string Hours, string SalaryBand, string ChannelText, string Budget, string Reviewer, string Revision, string StatusLabel)
{
    public static OffboardingBundleView ToOffboardingBundleView(OffboardingBundle item)
    {
        return new OffboardingBundleView(
            item.Code,
            item.Name,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 40),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.SalaryBand,
            item.Channel,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Reviewer,
            item.Revision.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<OffboardingBundleView> ToOffboardingBundleViews(List<OffboardingBundle> items)
    {
        var views = new List<OffboardingBundleView>();
        foreach (var item in items)
        {
            views.Add(ToOffboardingBundleView(item));
        }
        return views;
    }

    public static double SumOffboardingBundleView(OffboardingBundleView view)
    {
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedBudget = double.Parse(view.Budget, CultureInfo.InvariantCulture);
        return parsedRate + parsedHours + parsedBudget;
    }
}
