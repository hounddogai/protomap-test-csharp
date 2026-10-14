namespace Acme.Workforce.Payrollops.RetroPay.Review;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a retro pay agreement.</summary>
public sealed record RetroPayAgreementView(string Code, string Name, string Ceiling, string EmployeeId, string FloorValue, string ContactEmail, string ManagerId, string Version, string StatusLabel)
{
    public static RetroPayAgreementView ToRetroPayAgreementView(RetroPayAgreement item)
    {
        return new RetroPayAgreementView(
            Text.Truncate(item.Code, 64),
            item.Name,
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 40),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.ContactEmail,
            item.ManagerId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayAgreementView> ToRetroPayAgreementViews(List<RetroPayAgreement> items)
    {
        var views = new List<RetroPayAgreementView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayAgreementView(item));
        }
        return views;
    }

    public static double SumRetroPayAgreementView(RetroPayAgreementView view)
    {
        var parsedCeiling = double.Parse(view.Ceiling, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValue, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedFloorValue + (double)parsedVersion;
    }
}
