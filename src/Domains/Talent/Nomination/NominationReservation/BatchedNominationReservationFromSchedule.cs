namespace Acme.Workforce.Talent.Nomination.NominationReservation;

using Acme.Workforce.Talent.CareerPath.PathSlot;

/// <summary>Functions of batched nomination reservation from schedule.</summary>
public static class BatchedNominationReservationFromSchedule
{
    public static Dictionary<string, double> SummarizeCappedCareerPathSchedulesForReservation(List<CappedCareerPathSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<BatchedNominationReservation> DeriveBatchedNominationReservationsFromSchedule(List<CappedCareerPathSchedule> items)
    {
        var outValue = new List<BatchedNominationReservation>();
        foreach (var item in items)
        {
            var derived = BatchedNominationReservation.NewBatchedNominationReservation(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateBatchedNominationReservation(BatchedNominationReservation item)
    {
        return CappedCareerPathSchedulePolicy.NormalizeCappedCareerPathScheduleCode(item.Code);
    }
}
