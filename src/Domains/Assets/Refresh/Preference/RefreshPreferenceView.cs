namespace Acme.Workforce.Assets.Refresh.Preference;

using System.Globalization;

/// <summary>How screens and exports show a refresh preference.</summary>
public sealed record RefreshPreferenceView(string Code, string LabelText, string Badge, string Limit, string Phone, string Hours, string TitleText, string CurrencyText, string AttemptsText, string StatusLabel)
{
    public static RefreshPreferenceView ToRefreshPreferenceView(RefreshPreference item)
    {
        return new RefreshPreferenceView(
            item.Code,
            item.Label,
            item.Badge,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.Phone,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.Title,
            item.Currency,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshPreferenceView> ToRefreshPreferenceViews(List<RefreshPreference> items)
    {
        var views = new List<RefreshPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshPreferenceView(item));
        }
        return views;
    }

    public static double SumRefreshPreferenceView(RefreshPreferenceView view)
    {
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedLimit + parsedHours + (double)parsedAttempts;
    }
}
