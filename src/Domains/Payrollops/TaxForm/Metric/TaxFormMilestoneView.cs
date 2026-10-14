namespace Acme.Workforce.Payrollops.TaxForm.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tax form milestone.</summary>
public sealed record TaxFormMilestoneView(string Code, string TitleText, string ChannelText, string Minimum, string Maximum, string Location, string ExternalRefText, string Ratio, string Visible, string StatusLabel)
{
    public static TaxFormMilestoneView ToTaxFormMilestoneView(TaxFormMilestone item)
    {
        return new TaxFormMilestoneView(
            item.Code,
            item.Title,
            Text.Truncate(item.Channel, 120),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.ExternalRef, 120),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TaxFormMilestoneView> ToTaxFormMilestoneViews(List<TaxFormMilestone> items)
    {
        var views = new List<TaxFormMilestoneView>();
        foreach (var item in items)
        {
            views.Add(ToTaxFormMilestoneView(item));
        }
        return views;
    }

    public static double SumTaxFormMilestoneView(TaxFormMilestoneView view)
    {
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedMinimum + parsedMaximum + parsedRatio;
    }
}
