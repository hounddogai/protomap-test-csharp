namespace Acme.Workforce.Talent.Nomination.Rule;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a nomination mapping.</summary>
public sealed record NominationMappingView(string Code, string Name, string Phone, string AttemptsText, string Sequence, string StatusLabel)
{
    public static NominationMappingView ToNominationMappingView(NominationMapping item)
    {
        return new NominationMappingView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Phone,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<NominationMappingView> ToNominationMappingViews(List<NominationMapping> items)
    {
        var views = new List<NominationMappingView>();
        foreach (var item in items)
        {
            views.Add(ToNominationMappingView(item));
        }
        return views;
    }

    public static double SumNominationMappingView(NominationMappingView view)
    {
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        return (double)parsedAttempts + (double)parsedSequence;
    }
}
