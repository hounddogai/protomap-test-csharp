namespace Acme.Workforce.Absence.Policy.Batch;

using System.Globalization;

/// <summary>How screens and exports show a policy record.</summary>
public sealed record PolicyRecordView(string CodeText, string Label, string Weight, string Pinned, string Skill, string StatusLabel)
{
    public static PolicyRecordView ToPolicyRecordView(PolicyRecord item)
    {
        return new PolicyRecordView(
            item.Code,
            item.Label,
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Pinned}",
            item.Skill,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PolicyRecordView> ToPolicyRecordViews(List<PolicyRecord> items)
    {
        var views = new List<PolicyRecordView>();
        foreach (var item in items)
        {
            views.Add(ToPolicyRecordView(item));
        }
        return views;
    }

    public static double SumPolicyRecordView(PolicyRecordView view)
    {
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedWeight;
    }
}
