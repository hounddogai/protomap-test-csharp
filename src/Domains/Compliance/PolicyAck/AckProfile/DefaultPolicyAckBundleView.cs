namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default policy ack bundle.</summary>
public sealed record DefaultPolicyAckBundleView(string CodeText, string Title, string Name, string BudgetText, string RateText, string Location, string SummaryText, string Score, string Margin, string StatusLabel)
{
    public static DefaultPolicyAckBundleView ToDefaultPolicyAckBundleView(DefaultPolicyAckBundle item)
    {
        return new DefaultPolicyAckBundleView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Name,
            item.Budget.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 64),
            Text.Truncate(item.Summary, 64),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPolicyAckBundleView> ToDefaultPolicyAckBundleViews(List<DefaultPolicyAckBundle> items)
    {
        var views = new List<DefaultPolicyAckBundleView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPolicyAckBundleView(item));
        }
        return views;
    }

    public static double SumDefaultPolicyAckBundleView(DefaultPolicyAckBundleView view)
    {
        var parsedBudget = double.Parse(view.BudgetText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return parsedBudget + parsedRate + parsedScore;
    }
}
