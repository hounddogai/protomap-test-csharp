namespace Acme.Workforce.Absence.Accrual.Cycle;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a accrual preference.</summary>
public sealed record AccrualPreferenceView(string Code, string TitleText, string FloorValue, string UpdatedAt, string MaximumText, string Name, string Department, string ContactEmail, string RatingText, string Locale, string StatusLabel)
{
    public static AccrualPreferenceView ToAccrualPreferenceView(AccrualPreference item)
    {
        return new AccrualPreferenceView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Name,
            Text.Truncate(item.Department, 64),
            item.ContactEmail,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AccrualPreferenceView> ToAccrualPreferenceViews(List<AccrualPreference> items)
    {
        var views = new List<AccrualPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToAccrualPreferenceView(item));
        }
        return views;
    }

    public static double SumAccrualPreferenceView(AccrualPreferenceView view)
    {
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        return parsedFloorValue + (double)parsedUpdatedAt + parsedMaximum;
    }
}
