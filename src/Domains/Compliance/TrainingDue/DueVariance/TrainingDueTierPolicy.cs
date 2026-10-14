namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of training due tier policy.</summary>
public static class TrainingDueTierPolicy
{
    /// <summary>Puts a training due tier code in its canonical TDT- form.</summary>
    public static string NormalizeTrainingDueTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TDT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TDT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTrainingDueTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
