namespace Acme.Workforce.Payrollops.Deduction.Profile;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary deduction profile.</summary>
public sealed record PrimaryDeductionProfileView(string Code, string Title, string TenureMonths, string Capacity, string AttemptsText, string Location, string Weight, string Team, string Retries, string StatusLabel)
{
    public static PrimaryDeductionProfileView ToPrimaryDeductionProfileView(PrimaryDeductionProfile item)
    {
        return new PrimaryDeductionProfileView(
            Text.Truncate(item.Code, 80),
            item.Title,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Location, 120),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            item.Team,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryDeductionProfileView> ToPrimaryDeductionProfileViews(List<PrimaryDeductionProfile> items)
    {
        var views = new List<PrimaryDeductionProfileView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryDeductionProfileView(item));
        }
        return views;
    }

    public static double SumPrimaryDeductionProfileView(PrimaryDeductionProfileView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonths, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + (double)parsedCapacity + (double)parsedAttempts;
    }
}
