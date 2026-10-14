namespace Acme.Workforce.Contractors.StatementOfWork.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual statement of work profile.</summary>
public sealed record ManualStatementOfWorkProfileView(string Code, string Title, string JobCode, string ScoreText, string Margin, string Visible, string Pinned, string Quantity, string Priority, string StatusLabel)
{
    public static ManualStatementOfWorkProfileView ToManualStatementOfWorkProfileView(ManualStatementOfWorkProfile item)
    {
        return new ManualStatementOfWorkProfileView(
            item.Code,
            item.Title,
            Text.Truncate(item.JobCode, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Visible}",
            $"{item.Pinned}",
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Priority.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualStatementOfWorkProfileView> ToManualStatementOfWorkProfileViews(List<ManualStatementOfWorkProfile> items)
    {
        var views = new List<ManualStatementOfWorkProfileView>();
        foreach (var item in items)
        {
            views.Add(ToManualStatementOfWorkProfileView(item));
        }
        return views;
    }

    public static double SumManualStatementOfWorkProfileView(ManualStatementOfWorkProfileView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedScore + parsedMargin + (double)parsedQuantity;
    }
}
