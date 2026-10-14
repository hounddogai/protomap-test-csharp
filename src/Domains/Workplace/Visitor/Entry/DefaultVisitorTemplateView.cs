namespace Acme.Workforce.Workplace.Visitor.Entry;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default visitor template.</summary>
public sealed record DefaultVisitorTemplateView(string Code, string NameText, string PriorityText, string ArchivedText, string Country, string Sequence, string StatusLabel)
{
    public static DefaultVisitorTemplateView ToDefaultVisitorTemplateView(DefaultVisitorTemplate item)
    {
        return new DefaultVisitorTemplateView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Country,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultVisitorTemplateView> ToDefaultVisitorTemplateViews(List<DefaultVisitorTemplate> items)
    {
        var views = new List<DefaultVisitorTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultVisitorTemplateView(item));
        }
        return views;
    }

    public static double SumDefaultVisitorTemplateView(DefaultVisitorTemplateView view)
    {
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedPriority + (double)parsedSequence;
    }
}
