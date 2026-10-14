namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient retro pay preference.</summary>
public sealed record LenientRetroPayPreferenceView(string CodeText, string TitleText, string Sequence, string Location, string Phone, string StatusLabel)
{
    public static LenientRetroPayPreferenceView ToLenientRetroPayPreferenceView(LenientRetroPayPreference item)
    {
        return new LenientRetroPayPreferenceView(
            item.Code,
            item.Title,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Location,
            Text.Truncate(item.Phone, 120),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientRetroPayPreferenceView> ToLenientRetroPayPreferenceViews(List<LenientRetroPayPreference> items)
    {
        var views = new List<LenientRetroPayPreferenceView>();
        foreach (var item in items)
        {
            views.Add(ToLenientRetroPayPreferenceView(item));
        }
        return views;
    }

    public static double SumLenientRetroPayPreferenceView(LenientRetroPayPreferenceView view)
    {
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedSequence;
    }
}
