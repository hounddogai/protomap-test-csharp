namespace Acme.Workforce.Compliance.Exception.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a exception entry.</summary>
public sealed record ExceptionEntryView(string CodeText, string Title, string Source, string Skill, string Region, string Discount, string UnitText, string Weight, string CostCenterText, string Location, string Archived, string StatusLabel)
{
    public static ExceptionEntryView ToExceptionEntryView(ExceptionEntry item)
    {
        return new ExceptionEntryView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Source,
            Text.Truncate(item.Skill, 64),
            item.Region,
            item.Discount.ToString("F2", CultureInfo.InvariantCulture),
            item.Unit,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Location,
            $"{item.Archived}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ExceptionEntryView> ToExceptionEntryViews(List<ExceptionEntry> items)
    {
        var views = new List<ExceptionEntryView>();
        foreach (var item in items)
        {
            views.Add(ToExceptionEntryView(item));
        }
        return views;
    }

    public static double SumExceptionEntryView(ExceptionEntryView view)
    {
        var parsedDiscount = double.Parse(view.Discount, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedDiscount + parsedWeight;
    }
}
