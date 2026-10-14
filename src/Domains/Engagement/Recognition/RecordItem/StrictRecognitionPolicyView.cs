namespace Acme.Workforce.Engagement.Recognition.RecordItem;

using System.Globalization;

/// <summary>How screens and exports show a strict recognition policy.</summary>
public sealed record StrictRecognitionPolicyView(string Code, string Name, string VisibleText, string Country, string UpdatedAt, string ContactEmailText, string NotesText, string Minimum, string ChecksumText, string CostCenter, string StatusLabel)
{
    public static StrictRecognitionPolicyView ToStrictRecognitionPolicyView(StrictRecognitionPolicy item)
    {
        return new StrictRecognitionPolicyView(
            item.Code,
            item.Name,
            $"{item.Visible}",
            item.Country,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.Notes,
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            item.Checksum,
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<StrictRecognitionPolicyView> ToStrictRecognitionPolicyViews(List<StrictRecognitionPolicy> items)
    {
        var views = new List<StrictRecognitionPolicyView>();
        foreach (var item in items)
        {
            views.Add(ToStrictRecognitionPolicyView(item));
        }
        return views;
    }

    public static double SumStrictRecognitionPolicyView(StrictRecognitionPolicyView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMinimum;
    }
}
