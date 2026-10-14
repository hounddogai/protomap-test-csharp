namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a manual reorg profile.</summary>
public sealed record ManualReorgProfileView(string Code, string Name, string Fte, string Grade, string AttemptsText, string StatusLabel)
{
    public static ManualReorgProfileView ToManualReorgProfileView(ManualReorgProfile item)
    {
        return new ManualReorgProfileView(
            item.Code,
            Text.Truncate(item.Name, 40),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            item.Grade,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ManualReorgProfileView> ToManualReorgProfileViews(List<ManualReorgProfile> items)
    {
        var views = new List<ManualReorgProfileView>();
        foreach (var item in items)
        {
            views.Add(ToManualReorgProfileView(item));
        }
        return views;
    }

    public static double SumManualReorgProfileView(ManualReorgProfileView view)
    {
        var parsedFte = double.Parse(view.Fte, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return parsedFte + (double)parsedAttempts;
    }
}
