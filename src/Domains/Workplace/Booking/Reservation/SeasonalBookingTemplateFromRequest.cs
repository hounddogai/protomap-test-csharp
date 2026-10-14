namespace Acme.Workforce.Workplace.Booking.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Audit;

/// <summary>Functions of seasonal booking template from request.</summary>
public static class SeasonalBookingTemplateFromRequest
{
    public static Dictionary<string, double> SummarizeFallbackFloorPlanRequestsForTemplate(List<FallbackFloorPlanRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<SeasonalBookingTemplate> DeriveSeasonalBookingTemplatesFromRequest(List<FallbackFloorPlanRequest> items)
    {
        var outValue = new List<SeasonalBookingTemplate>();
        foreach (var item in items)
        {
            var derived = SeasonalBookingTemplate.NewSeasonalBookingTemplate(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceSeasonalBookingTemplate(SeasonalBookingTemplate item)
    {
        return FloorPlanSegmentPolicy.FloorPlanSegmentWeight(6, true);
    }
}
