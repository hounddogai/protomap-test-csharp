namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Workplace.Desk.DeskProposal;

/// <summary>Functions of desk draft from exception.</summary>
public static class DeskDraftFromException
{
    public static Dictionary<string, double> SummarizeDeskExceptionsForDraft(List<DeskException> items)
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

    public static List<DeskDraft> DeriveDeskDraftsFromException(List<DeskException> items)
    {
        var outValue = new List<DeskDraft>();
        foreach (var item in items)
        {
            var derived = DeskDraft.NewDeskDraft(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectDeskDraft(DeskDraft item)
    {
        return FallbackDeskSlotPolicy.FallbackDeskSlotWeight(7, true);
    }
}
