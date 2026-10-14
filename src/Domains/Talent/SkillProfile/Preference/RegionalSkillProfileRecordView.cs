namespace Acme.Workforce.Talent.SkillProfile.Preference;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional skill profile record.</summary>
public sealed record RegionalSkillProfileRecordView(string Code, string LabelText, string Team, string WindowDaysText, string FloorValue, string Checksum, string QuantityText, string StatusLabel)
{
    public static RegionalSkillProfileRecordView ToRegionalSkillProfileRecordView(RegionalSkillProfileRecord item)
    {
        return new RegionalSkillProfileRecordView(
            item.Code,
            item.Label,
            item.Team,
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Checksum, 80),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSkillProfileRecordView> ToRegionalSkillProfileRecordViews(List<RegionalSkillProfileRecord> items)
    {
        var views = new List<RegionalSkillProfileRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSkillProfileRecordView(item));
        }
        return views;
    }

    public static double SumRegionalSkillProfileRecordView(RegionalSkillProfileRecordView view)
    {
        var parsedWindowDays = int.Parse(view.WindowDaysText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        return (double)parsedWindowDays + parsedFloorValue + (double)parsedQuantity;
    }
}
