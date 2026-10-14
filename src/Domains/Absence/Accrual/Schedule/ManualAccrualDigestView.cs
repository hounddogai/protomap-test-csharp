namespace Acme.Workforce.Absence.Accrual.Schedule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual accrual digest.</summary>
public sealed record ManualAccrualDigestView(string CodeText, string Name, string Location, string ExternalRefText, string StartsAt, string StatusLabel)
{
    public static ManualAccrualDigestView ToManualAccrualDigestView(ManualAccrualDigest item)
    {
        return new ManualAccrualDigestView(
            Text.Truncate(item.Code, 80),
            Text.Truncate(item.Name, 64),
            Text.Truncate(item.Location, 80),
            Text.Truncate(item.ExternalRef, 80),
            item.StartsAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualAccrualDigestView> ToManualAccrualDigestViews(List<ManualAccrualDigest> items)
    {
        var views = new List<ManualAccrualDigestView>();
        foreach (var item in items)
        {
            views.Add(ToManualAccrualDigestView(item));
        }
        return views;
    }

    public static double SumManualAccrualDigestView(ManualAccrualDigestView view)
    {
        var parsedStartsAt = int.Parse(view.StartsAt, CultureInfo.InvariantCulture);
        return (double)parsedStartsAt;
    }
}
