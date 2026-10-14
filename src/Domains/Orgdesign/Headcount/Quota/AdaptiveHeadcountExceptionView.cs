namespace Acme.Workforce.Orgdesign.Headcount.Quota;

using System.Globalization;

/// <summary>How screens and exports show a adaptive headcount exception.</summary>
public sealed record AdaptiveHeadcountExceptionView(string Code, string NameText, string Region, string LeaveDaysText, string OwnerId, string RatingText, string Rate, string StatusLabel)
{
    public static AdaptiveHeadcountExceptionView ToAdaptiveHeadcountExceptionView(AdaptiveHeadcountException item)
    {
        return new AdaptiveHeadcountExceptionView(
            item.Code,
            item.Name,
            item.Region,
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.OwnerId,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveHeadcountExceptionView> ToAdaptiveHeadcountExceptionViews(List<AdaptiveHeadcountException> items)
    {
        var views = new List<AdaptiveHeadcountExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveHeadcountExceptionView(item));
        }
        return views;
    }

    public static double SumAdaptiveHeadcountExceptionView(AdaptiveHeadcountExceptionView view)
    {
        var parsedLeaveDays = int.Parse(view.LeaveDaysText, CultureInfo.InvariantCulture);
        var parsedRating = double.Parse(view.RatingText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return (double)parsedLeaveDays + parsedRating + parsedRate;
    }
}
