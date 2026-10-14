namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Functions of batched nomination reservation from request.</summary>
public static class BatchedNominationReservationFromRequest
{
    public static Dictionary<string, double> SummarizeLenientMobilityRequestsForReservation(List<LenientMobilityRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<BatchedNominationReservation> DeriveBatchedNominationReservationsFromRequest(List<LenientMobilityRequest> items)
    {
        var outValue = new List<BatchedNominationReservation>();
        foreach (var item in items)
        {
            var derived = BatchedNominationReservation.NewBatchedNominationReservation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectBatchedNominationReservation(BatchedNominationReservation item)
    {
        return ManualMobilityRecordPolicy.NormalizeManualMobilityRecordCode(item.Code);
    }
}
