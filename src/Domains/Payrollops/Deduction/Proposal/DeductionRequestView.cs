namespace Acme.Workforce.Payrollops.Deduction.Proposal;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a deduction request.</summary>
public sealed record DeductionRequestView(string Code, string NameText, string CountryText, string Amount, string Minimum, string EmployeeIdText, string Version, string Team, string Region, string UnitText, string StatusLabel)
{
    public static DeductionRequestView ToDeductionRequestView(DeductionRequest item)
    {
        return new DeductionRequestView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.Country,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Minimum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.EmployeeId, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Team,
            Text.Truncate(item.Region, 80),
            item.Unit,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<DeductionRequestView> ToDeductionRequestViews(List<DeductionRequest> items)
    {
        var views = new List<DeductionRequestView>();
        foreach (var item in items)
        {
            views.Add(ToDeductionRequestView(item));
        }
        return views;
    }

    public static double SumDeductionRequestView(DeductionRequestView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedMinimum = double.Parse(view.Minimum, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedAmount + parsedMinimum + (double)parsedVersion;
    }
}
