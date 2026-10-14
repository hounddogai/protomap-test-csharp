namespace Acme.Workforce.Compensation.BonusPool.Claim;

using System.Globalization;

/// <summary>How screens and exports show a lenient bonus pool draft.</summary>
public sealed record LenientBonusPoolDraftView(string Code, string TitleText, string SalaryBandText, string AttemptsText, string Minimum, string Hours, string StatusLabel)
{
    public static LenientBonusPoolDraftView ToLenientBonusPoolDraftView(LenientBonusPoolDraft item)
    {
        return new LenientBonusPoolDraftView(
            item.Code,
            item.Title,
            item.SalaryBand,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientBonusPoolDraftView> ToLenientBonusPoolDraftViews(List<LenientBonusPoolDraft> items)
    {
        var views = new List<LenientBonusPoolDraftView>();
        foreach (var item in items)
        {
            views.Add(ToLenientBonusPoolDraftView(item));
        }
        return views;
    }

    public static double SumLenientBonusPoolDraftView(LenientBonusPoolDraftView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + parsedMinimum + parsedHours;
    }
}
