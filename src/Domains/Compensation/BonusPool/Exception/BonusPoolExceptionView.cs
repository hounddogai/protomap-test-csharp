namespace Acme.Workforce.Compensation.BonusPool.Exception;

using System.Globalization;

/// <summary>How screens and exports show a bonus pool exception.</summary>
public sealed record BonusPoolExceptionView(string Code, string Label, string ExternalRef, string AttemptsText, string RateText, string GraceDays, string StatusLabel)
{
    public static BonusPoolExceptionView ToBonusPoolExceptionView(BonusPoolException item)
    {
        return new BonusPoolExceptionView(
            item.Code,
            item.Label,
            item.ExternalRef,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BonusPoolExceptionView> ToBonusPoolExceptionViews(List<BonusPoolException> items)
    {
        var views = new List<BonusPoolExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToBonusPoolExceptionView(item));
        }
        return views;
    }

    public static double SumBonusPoolExceptionView(BonusPoolExceptionView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.RateText, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedRate + (double)parsedGraceDays;
    }
}
