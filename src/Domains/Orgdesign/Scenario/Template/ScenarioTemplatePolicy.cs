namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scenario template policy.</summary>
public static class ScenarioTemplatePolicy
{
    /// <summary>Puts a scenario template code in its canonical ST- form.</summary>
    public static string NormalizeScenarioTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ST-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ST-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeScenarioTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 26023.0, in order.</summary>
    public static int ScenarioTemplateBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26023.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
