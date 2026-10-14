namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a legacy exception agreement.</summary>
public sealed record LegacyExceptionAgreementView(string CodeText, string Name, string ThresholdText, string SkillText, string Title, string Locale, string Sequence, string ContactEmail, string GraceDays, string DurationMinutes, string StatusLabel)
{
    public static LegacyExceptionAgreementView ToLegacyExceptionAgreementView(LegacyExceptionAgreement item)
    {
        return new LegacyExceptionAgreementView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Skill, 40),
            Text.Truncate(item.Title, 40),
            item.Locale,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LegacyExceptionAgreementView> ToLegacyExceptionAgreementViews(List<LegacyExceptionAgreement> items)
    {
        var views = new List<LegacyExceptionAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToLegacyExceptionAgreementView(item));
        }
        return views;
    }

    public static double SumLegacyExceptionAgreementView(LegacyExceptionAgreementView view)
    {
        var parsedThreshold = double.Parse(view.ThresholdText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedSequence + (double)parsedGraceDays;
    }
}
