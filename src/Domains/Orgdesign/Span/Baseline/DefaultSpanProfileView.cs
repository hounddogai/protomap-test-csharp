namespace Acme.Workforce.Orgdesign.Span.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default span profile.</summary>
public sealed record DefaultSpanProfileView(string Code, string Title, string LabelText, string Notes, string DepartmentText, string PhoneText, string Weight, string Sequence, string VisibleText, string ThresholdText, string StatusLabel)
{
    public static DefaultSpanProfileView ToDefaultSpanProfileView(DefaultSpanProfile item)
    {
        return new DefaultSpanProfileView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Title, 80),
            item.Label,
            item.Notes,
            item.Department,
            item.Phone,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultSpanProfileView> ToDefaultSpanProfileViews(List<DefaultSpanProfile> items)
    {
        var views = new List<DefaultSpanProfileView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultSpanProfileView(item));
        }
        return views;
    }

    public static double SumDefaultSpanProfileView(DefaultSpanProfileView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        return parsedWeight + (double)parsedSequence + parsedThreshold;
    }
}
