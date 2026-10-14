namespace Acme.Workforce.Orgdesign.Position.Policy;

using Acme.Workforce.Orgdesign.Span.Exception;

/// <summary>Functions of position rollup from exception.</summary>
public static class PositionRollupFromException
{
    public static Dictionary<string, double> SummarizeSpanExceptionsForRollup(List<SpanException> items)
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

    public static List<PositionRollup> DerivePositionRollupsFromException(List<SpanException> items)
    {
        var outValue = new List<PositionRollup>();
        foreach (var item in items)
        {
            var derived = PositionRollup.NewPositionRollup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyPositionRollup(PositionRollup item)
    {
        return SpanExceptionPolicy.SpanExceptionWeight(5, true);
    }
}
