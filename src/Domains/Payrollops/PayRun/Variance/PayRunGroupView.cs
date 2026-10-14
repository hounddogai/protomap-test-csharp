namespace Acme.Workforce.Payrollops.PayRun.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pay run group.</summary>
public sealed record PayRunGroupView(string Code, string Label, string ReviewerText, string ContactEmail, string Currency, string LimitText, string MaximumText, string StatusLabel)
{
    public static PayRunGroupView ToPayRunGroupView(PayRunGroup item)
    {
        return new PayRunGroupView(
            item.Code,
            Text.Truncate(item.Label, 64),
            Text.Truncate(item.Reviewer, 40),
            Text.Truncate(item.ContactEmail, 80),
            Text.Truncate(item.Currency, 80),
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PayRunGroupView> ToPayRunGroupViews(List<PayRunGroup> items)
    {
        var views = new List<PayRunGroupView>();
        foreach (var item in items)
        {
            views.Add(ToPayRunGroupView(item));
        }
        return views;
    }

    public static double SumPayRunGroupView(PayRunGroupView view)
    {
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedMaximum;
    }
}
