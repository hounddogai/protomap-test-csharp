namespace Acme.Workforce.Engagement.PulseSurvey.Assignment;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a pulse survey assignment.</summary>
public sealed record PulseSurveyAssignmentView(string Code, string Name, string Phone, string Country, string LocaleText, string Visible, string Skill, string Title, string AmountText, string StatusLabel)
{
    public static PulseSurveyAssignmentView ToPulseSurveyAssignmentView(PulseSurveyAssignment item)
    {
        return new PulseSurveyAssignmentView(
            item.Code,
            item.Name,
            Text.Truncate(item.Phone, 64),
            item.Country,
            item.Locale,
            $"{item.Visible}",
            item.Skill,
            item.Title,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PulseSurveyAssignmentView> ToPulseSurveyAssignmentViews(List<PulseSurveyAssignment> items)
    {
        var views = new List<PulseSurveyAssignmentView>();
        foreach (var item in items)
        {
            views.Add(ToPulseSurveyAssignmentView(item));
        }
        return views;
    }

    public static double SumPulseSurveyAssignmentView(PulseSurveyAssignmentView view)
    {
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return parsedAmount;
    }
}
