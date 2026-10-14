namespace Acme.Workforce.Talent.CareerPath.Draft;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path draft.</summary>
public sealed record CareerPathDraftView(string Code, string Label, string SequenceText, string ContactEmailText, string SegmentText, string VersionText, string Department, string AmountText, string Name, string Fte, string StatusLabel)
{
    public static CareerPathDraftView ToCareerPathDraftView(CareerPathDraft item)
    {
        return new CareerPathDraftView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Segment,
            item.Version.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Department, 40),
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 64),
            item.Fte.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathDraftView> ToCareerPathDraftViews(List<CareerPathDraft> items)
    {
        var views = new List<CareerPathDraftView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathDraftView(item));
        }
        return views;
    }

    public static double SumCareerPathDraftView(CareerPathDraftView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedAmount = double.Parse(view.AmountText, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedVersion + parsedAmount;
    }
}
