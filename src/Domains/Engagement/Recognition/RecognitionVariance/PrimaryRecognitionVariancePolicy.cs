namespace Acme.Workforce.Engagement.Recognition.RecognitionVariance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of primary recognition variance policy.</summary>
public static class PrimaryRecognitionVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 90988.0, in order.</summary>
    public static int PrimaryRecognitionVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90988.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double PrimaryRecognitionVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a primary recognition variance score falls in.</summary>
    public static string ClassifyPrimaryRecognitionVariance(double score)
    {
        if (score < 10.9)
        {
            return "low";
        }
        if (score < 31.3)
        {
            return "minimal";
        }
        if (score < 32.1)
        {
            return "elevated";
        }
        if (score < 40.3)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Puts a primary recognition variance code in its canonical PRV- form.</summary>
    public static string NormalizePrimaryRecognitionVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
