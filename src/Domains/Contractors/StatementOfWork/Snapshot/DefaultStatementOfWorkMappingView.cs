namespace Acme.Workforce.Contractors.StatementOfWork.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default statement of work mapping.</summary>
public sealed record DefaultStatementOfWorkMappingView(string Code, string LabelText, string Pinned, string SourceText, string Weight, string StatusLabel)
{
    public static DefaultStatementOfWorkMappingView ToDefaultStatementOfWorkMappingView(DefaultStatementOfWorkMapping item)
    {
        return new DefaultStatementOfWorkMappingView(
            Text.Truncate(item.Code, 120),
            Text.Truncate(item.Label, 64),
            $"{item.Pinned}",
            item.Source,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultStatementOfWorkMappingView> ToDefaultStatementOfWorkMappingViews(List<DefaultStatementOfWorkMapping> items)
    {
        var views = new List<DefaultStatementOfWorkMappingView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultStatementOfWorkMappingView(item));
        }
        return views;
    }

    public static double SumDefaultStatementOfWorkMappingView(DefaultStatementOfWorkMappingView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
