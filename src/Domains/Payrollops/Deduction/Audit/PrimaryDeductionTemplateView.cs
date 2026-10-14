namespace Acme.Workforce.Payrollops.Deduction.Audit;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a primary deduction template.</summary>
public sealed record PrimaryDeductionTemplateView(string Code, string Title, string ScoreText, string Phone, string LeaveDays, string Weight, string Currency, string CapacityText, string Archived, string Unit, string GraceDays, string StatusLabel)
{
    public static PrimaryDeductionTemplateView ToPrimaryDeductionTemplateView(PrimaryDeductionTemplate item)
    {
        return new PrimaryDeductionTemplateView(
            item.Code,
            item.Title,
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Phone, 64),
            item.LeaveDays.ToString(CultureInfo.InvariantCulture),
            item.Weight.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Currency, 80),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            $"{item.Archived}",
            item.Unit,
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<PrimaryDeductionTemplateView> ToPrimaryDeductionTemplateViews(List<PrimaryDeductionTemplate> items)
    {
        var views = new List<PrimaryDeductionTemplateView>();
        foreach (var item in items)
        {
            views.Add(ToPrimaryDeductionTemplateView(item));
        }
        return views;
    }

    public static double SumPrimaryDeductionTemplateView(PrimaryDeductionTemplateView view)
    {
        var parsedScore = double.Parse(view.ScoreText, CultureInfo.InvariantCulture);
        var parsedLeaveDays = int.Parse(view.LeaveDays, CultureInfo.InvariantCulture);
        var parsedWeight = double.Parse(view.Weight, CultureInfo.InvariantCulture);
        return parsedScore + (double)parsedLeaveDays + parsedWeight;
    }
}
