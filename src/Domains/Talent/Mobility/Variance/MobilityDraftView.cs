namespace Acme.Workforce.Talent.Mobility.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a mobility draft.</summary>
public sealed record MobilityDraftView(string CodeText, string NameText, string Unit, string RetriesText, string Grade, string OwnerId, string Region, string ExternalRefText, string VisibleText, string StatusLabel)
{
    public static MobilityDraftView ToMobilityDraftView(MobilityDraft item)
    {
        return new MobilityDraftView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Unit, 64),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            item.OwnerId,
            item.Region,
            item.ExternalRef,
            $"{item.Visible}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<MobilityDraftView> ToMobilityDraftViews(List<MobilityDraft> items)
    {
        var views = new List<MobilityDraftView>();
        foreach (var item in items)
        {
            views.Add(ToMobilityDraftView(item));
        }
        return views;
    }

    public static double SumMobilityDraftView(MobilityDraftView view)
    {
        var parsedRetries = int.Parse(view.RetriesText, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
