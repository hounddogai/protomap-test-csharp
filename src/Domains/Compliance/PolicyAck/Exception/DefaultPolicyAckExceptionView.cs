namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a default policy ack exception.</summary>
public sealed record DefaultPolicyAckExceptionView(string CodeText, string NameText, string GraceDays, string Notes, string HoursText, string CreatedAt, string WeightText, string Cost, string OwnerId, string WindowDays, string StatusLabel)
{
    public static DefaultPolicyAckExceptionView ToDefaultPolicyAckExceptionView(DefaultPolicyAckException item)
    {
        return new DefaultPolicyAckExceptionView(
            Text.Truncate(item.Code, 120),
            item.Name,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.OwnerId, 64),
            item.WindowDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DefaultPolicyAckExceptionView> ToDefaultPolicyAckExceptionViews(List<DefaultPolicyAckException> items)
    {
        var views = new List<DefaultPolicyAckExceptionView>();
        foreach (var item in items)
        {
            views.Add(ToDefaultPolicyAckExceptionView(item));
        }
        return views;
    }

    public static double SumDefaultPolicyAckExceptionView(DefaultPolicyAckExceptionView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.HoursText, CultureInfo.InvariantCulture);
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + parsedHours + (double)parsedCreatedAt;
    }
}
