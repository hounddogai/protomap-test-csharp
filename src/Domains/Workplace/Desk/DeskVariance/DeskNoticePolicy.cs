namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of desk notice policy.</summary>
public static class DeskNoticePolicy
{
    public static List<string> MergeDeskNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string DeskNoticeBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Puts a desk notice code in its canonical DN- form.</summary>
    public static string NormalizeDeskNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
