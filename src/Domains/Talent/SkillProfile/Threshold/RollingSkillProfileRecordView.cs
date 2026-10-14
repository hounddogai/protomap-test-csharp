namespace Acme.Workforce.Talent.SkillProfile.Threshold;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a rolling skill profile record.</summary>
public sealed record RollingSkillProfileRecordView(string Code, string Name, string MaximumText, string CostCenter, string EmployeeIdText, string Category, string Rate, string StatusLabel)
{
    public static RollingSkillProfileRecordView ToRollingSkillProfileRecordView(RollingSkillProfileRecord item)
    {
        return new RollingSkillProfileRecordView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.CostCenter,
            item.EmployeeId,
            item.Category,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RollingSkillProfileRecordView> ToRollingSkillProfileRecordViews(List<RollingSkillProfileRecord> items)
    {
        var views = new List<RollingSkillProfileRecordView>();
        foreach (var item in items)
        {
            views.Add(ToRollingSkillProfileRecordView(item));
        }
        return views;
    }

    public static double SumRollingSkillProfileRecordView(RollingSkillProfileRecordView view)
    {
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedRate;
    }
}
