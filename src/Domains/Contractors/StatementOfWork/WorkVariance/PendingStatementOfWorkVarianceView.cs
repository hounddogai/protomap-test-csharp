namespace Acme.Workforce.Contractors.StatementOfWork.WorkVariance;

using System.Globalization;

/// <summary>How screens and exports show a pending statement of work variance.</summary>
public sealed record PendingStatementOfWorkVarianceView(string CodeText, string TitleText, string CeilingText, string ManagerIdText, string ChannelText, string Department, string Country, string StatusLabel)
{
    public static PendingStatementOfWorkVarianceView ToPendingStatementOfWorkVarianceView(PendingStatementOfWorkVariance item)
    {
        return new PendingStatementOfWorkVarianceView(
            item.Code,
            item.Title,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.ManagerId,
            item.Channel,
            item.Department,
            item.Country,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PendingStatementOfWorkVarianceView> ToPendingStatementOfWorkVarianceViews(List<PendingStatementOfWorkVariance> items)
    {
        var views = new List<PendingStatementOfWorkVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToPendingStatementOfWorkVarianceView(item));
        }
        return views;
    }

    public static double SumPendingStatementOfWorkVarianceView(PendingStatementOfWorkVarianceView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        return parsedCeiling;
    }
}
