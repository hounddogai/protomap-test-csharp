namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of certification reservation policy.</summary>
public static class CertificationReservationPolicy
{
    public static List<string> MergeCertificationReservationLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 82791.0, in order.</summary>
    public static int CertificationReservationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82791.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a certification reservation score falls in.</summary>
    public static string ClassifyCertificationReservation(double score)
    {
        if (score < 10.2)
        {
            return "medium";
        }
        if (score < 50.3)
        {
            return "watch";
        }
        if (score < 54.8)
        {
            return "high";
        }
        if (score < 86.1)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCertificationReservation()
    {
        var policy = Retry.DefaultPolicy("certification-reservation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
