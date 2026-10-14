namespace Acme.Workforce.Engagement.PulseSurvey.Template;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey notice.</summary>
public sealed record PulseSurveyNoticeView(string CodeText, string Title, string Unit, string Description, string Quantity, string StatusLabel)
{
    public static PulseSurveyNoticeView ToPulseSurveyNoticeView(PulseSurveyNotice item)
    {
        return new PulseSurveyNoticeView(
            item.Code,
            Text.Truncate(item.Title, 80),
            Text.Truncate(item.Unit, 40),
            item.Description,
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyNoticeView> ToPulseSurveyNoticeViews(List<PulseSurveyNotice> items)
    {
        var views = new List<PulseSurveyNoticeView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyNoticeView(item));
        }
        return views;
    }

    public static double SumPulseSurveyNoticeView(PulseSurveyNoticeView view)
    {
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedQuantity;
    }
}
