namespace Acme.Workforce.Talent.CareerPath.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a career path policy.</summary>
public sealed record CareerPathPolicyView(string Code, string LabelText, string SequenceText, string EndsAt, string Location, string Segment, string GraceDays, string TitleText, string OwnerIdText, string Maximum, string Checksum, string StatusLabel)
{
    public static CareerPathPolicyView ToCareerPathPolicyView(CareerPathPolicy item)
    {
        return new CareerPathPolicyView(
            item.Code,
            item.Label,
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 80),
            item.Segment,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Title,
            item.OwnerId,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CareerPathPolicyView> ToCareerPathPolicyViews(List<CareerPathPolicy> items)
    {
        var views = new List<CareerPathPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToCareerPathPolicyView(item));
        }
        return views;
    }

    public static double SumCareerPathPolicyView(CareerPathPolicyView view)
    {
        var parsedSequence = int.Parse(view.SequenceText, CultureInfo.InvariantCulture);
        var parsedEndsAt = int.Parse(view.EndsAt, CultureInfo.InvariantCulture);
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        return (double)parsedSequence + (double)parsedEndsAt + (double)parsedGraceDays;
    }
}
