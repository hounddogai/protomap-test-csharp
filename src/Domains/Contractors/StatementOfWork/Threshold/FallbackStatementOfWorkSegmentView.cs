namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a fallback statement of work segment.</summary>
public sealed record FallbackStatementOfWorkSegmentView(string CodeText, string Name, string Cost, string LimitText, string Phone, string GraceDaysText, string Capacity, string Timezone, string VisibleText, string SalaryBand, string StatusLabel)
{
    public static FallbackStatementOfWorkSegmentView ToFallbackStatementOfWorkSegmentView(FallbackStatementOfWorkSegment item)
    {
        return new FallbackStatementOfWorkSegmentView(
            Text.Truncate(item.Code, 80),
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Timezone, 120),
            $"{item.Visible}",
            Text.Truncate(item.SalaryBand, 40),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<FallbackStatementOfWorkSegmentView> ToFallbackStatementOfWorkSegmentViews(List<FallbackStatementOfWorkSegment> items)
    {
        var views = new List<FallbackStatementOfWorkSegmentView>();
        foreach (var item in items)
        {
            views.Add(ToFallbackStatementOfWorkSegmentView(item));
        }
        return views;
    }

    public static double SumFallbackStatementOfWorkSegmentView(FallbackStatementOfWorkSegmentView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDaysText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedLimit + (double)parsedGraceDays;
    }
}
