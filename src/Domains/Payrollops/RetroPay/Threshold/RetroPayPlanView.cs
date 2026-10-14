namespace Acme.Workforce.Payrollops.RetroPay.Threshold;

using System.Globalization;

/// <summary>How screens and exports show a retro pay plan.</summary>
public sealed record RetroPayPlanView(string Code, string Name, string Cost, string Sequence, string Quantity, string TeamText, string UpdatedAtText, string Ratio, string EndsAt, string Notes, string StatusLabel)
{
    public static RetroPayPlanView ToRetroPayPlanView(RetroPayPlan item)
    {
        return new RetroPayPlanView(
            item.Code,
            item.Name,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Sequence.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Team,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Notes,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RetroPayPlanView> ToRetroPayPlanViews(List<RetroPayPlan> items)
    {
        var views = new List<RetroPayPlanView>();
        foreach (var item in items)
        {
            views.Add(ToRetroPayPlanView(item));
        }
        return views;
    }

    public static double SumRetroPayPlanView(RetroPayPlanView view)
    {
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        var parsedSequence = int.Parse(view.Sequence, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedSequence + (double)parsedQuantity;
    }
}
