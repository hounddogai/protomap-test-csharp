namespace Acme.Workforce.Workplace.Visitor.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a visitor template.</summary>
public sealed record VisitorTemplateView(string Code, string NameText, string Ratio, string Department, string Visible, string JobCode, string StatusLabel)
{
    public static VisitorTemplateView ToVisitorTemplateView(VisitorTemplate item)
    {
        return new VisitorTemplateView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.Department,
            $"{item.Visible}",
            Text.Truncate(item.JobCode, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<VisitorTemplateView> ToVisitorTemplateViews(List<VisitorTemplate> items)
    {
        var views = new List<VisitorTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToVisitorTemplateView(item));
        }
        return views;
    }

    public static double SumVisitorTemplateView(VisitorTemplateView view)
    {
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return parsedRatio;
    }
}
