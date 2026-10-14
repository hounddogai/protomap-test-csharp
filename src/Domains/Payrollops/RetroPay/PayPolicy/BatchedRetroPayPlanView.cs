namespace Acme.Workforce.Payrollops.RetroPay.PayPolicy;

using System.Globalization;

/// <summary>How screens and exports show a batched retro pay plan.</summary>
public sealed record BatchedRetroPayPlanView(string Code, string NameText, string ManagerId, string Active, string ExpiresAt, string CostCenterText, string AttemptsText, string Score, string StatusLabel)
{
    public static BatchedRetroPayPlanView ToBatchedRetroPayPlanView(BatchedRetroPayPlan item)
    {
        return new BatchedRetroPayPlanView(
            item.Code,
            item.Name,
            item.ManagerId,
            $"{item.Active}",
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Score.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRetroPayPlanView> ToBatchedRetroPayPlanViews(List<BatchedRetroPayPlan> items)
    {
        var views = new List<BatchedRetroPayPlanView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRetroPayPlanView(item));
        }
        return views;
    }

    public static double SumBatchedRetroPayPlanView(BatchedRetroPayPlanView view)
    {
        var parsedExpiresAt = int.Parse(view.ExpiresAt, CultureInfo.InvariantCulture);
        var parsedAttempts = int.Parse(view.AttemptsText, CultureInfo.InvariantCulture);
        var parsedScore = double.Parse(view.Score, CultureInfo.InvariantCulture);
        return (double)parsedExpiresAt + (double)parsedAttempts + parsedScore;
    }
}
