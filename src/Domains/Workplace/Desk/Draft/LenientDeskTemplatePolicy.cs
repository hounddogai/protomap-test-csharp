namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient desk template policy.</summary>
public static class LenientDeskTemplatePolicy
{
    public static string LenientDeskTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 1976.0, in order.</summary>
    public static int LenientDeskTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1976.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a lenient desk template score falls in.</summary>
    public static string ClassifyLenientDeskTemplate(double score)
    {
        if (score < 15.2)
        {
            return "ok";
        }
        if (score < 50.7)
        {
            return "watch";
        }
        if (score < 59.9)
        {
            return "elevated";
        }
        return "critical";
    }

    public static List<string> MergeLenientDeskTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
