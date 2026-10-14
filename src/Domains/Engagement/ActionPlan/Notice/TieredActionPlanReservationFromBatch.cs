namespace Acme.Workforce.Engagement.ActionPlan.Notice;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Functions of tiered action plan reservation from batch.</summary>
public static class TieredActionPlanReservationFromBatch
{
    public static Dictionary<string, double> SummarizeLenientRecognitionBatchesForReservation(List<LenientRecognitionBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<TieredActionPlanReservation> DeriveTieredActionPlanReservationsFromBatch(List<LenientRecognitionBatch> items)
    {
        var outValue = new List<TieredActionPlanReservation>();
        foreach (var item in items)
        {
            var derived = TieredActionPlanReservation.NewTieredActionPlanReservation(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeTieredActionPlanReservation(TieredActionPlanReservation item)
    {
        return LenientRecognitionBatchPolicy.ClassifyLenientRecognitionBatch(item.NetVolume());
    }
}
