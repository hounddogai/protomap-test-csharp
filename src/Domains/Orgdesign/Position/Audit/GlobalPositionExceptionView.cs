namespace Acme.Workforce.Orgdesign.Position.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a global position exception.</summary>
public sealed record GlobalPositionExceptionView(string Code, string Name, string Quantity, string GradeText, string PriorityText, string StatusLabel)
{
    public static GlobalPositionExceptionView ToGlobalPositionExceptionView(GlobalPositionException item)
    {
        return new GlobalPositionExceptionView(
            item.Code,
            item.Name,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Grade, 80),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GlobalPositionExceptionView> ToGlobalPositionExceptionViews(List<GlobalPositionException> items)
    {
        var views = new List<GlobalPositionExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToGlobalPositionExceptionView(item));
        }
        return views;
    }

    public static double SumGlobalPositionExceptionView(GlobalPositionExceptionView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        var parsedPriority = int.Parse(view.PriorityText, CultureInfo.InvariantCulture);
        return (double)parsedQuantity + (double)parsedPriority;
    }
}
