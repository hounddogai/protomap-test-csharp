namespace Acme.Workforce.Engagement.TownHall.HallRule;

using Acme.Workforce.Engagement.PulseSurvey.Group;

/// <summary>Functions of town hall record from reservation.</summary>
public static class TownHallRecordFromReservation
{
    public static Dictionary<string, double> SummarizePulseSurveyReservationsForRecord(List<PulseSurveyReservation> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<TownHallRecord> DeriveTownHallRecordsFromReservation(List<PulseSurveyReservation> items)
    {
        var outValue = new List<TownHallRecord>();
        foreach (var item in items)
        {
            var derived = TownHallRecord.NewTownHallRecord(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceTownHallRecord(TownHallRecord item)
    {
        return PulseSurveyReservationPolicy.ClassifyPulseSurveyReservation(item.WeightedValue());
    }
}
