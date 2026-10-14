namespace Acme.Workforce.Compliance.Certification.CertificationSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling certification summary is in its life cycle.</summary>
public enum RollingCertificationSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A rolling certification summary as the certification team keeps it.</summary>
public sealed class RollingCertificationSummary(string code, string name, List<string> tags, string notes, string jobCode, double rate, double score, int attempts, RollingCertificationSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public string Notes { get; set; } = notes;
    public string JobCode { get; set; } = jobCode;
    public double Rate { get; set; } = rate;
    public double Score { get; set; } = score;
    public int Attempts { get; set; } = attempts;
    public RollingCertificationSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a rolling certification summary in status idle.</summary>
    public static RollingCertificationSummary NewRollingCertificationSummary(string code, string name)
    {
        return new RollingCertificationSummary(
            code,
            name,
            new List<string>(),
            "",
            "",
            0.0,
            0.0,
            0,
            RollingCertificationSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCertificationSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCertificationSummaryStatus next)
    {
        switch (Status)
        {
            case RollingCertificationSummaryStatus.Idle:
                switch (next)
                {
                    case RollingCertificationSummaryStatus.Ready:
                        return true;
                }
                return false;
            case RollingCertificationSummaryStatus.Warming:
                switch (next)
                {
                    case RollingCertificationSummaryStatus.Ready:
                    case RollingCertificationSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case RollingCertificationSummaryStatus.Ready:
                switch (next)
                {
                    case RollingCertificationSummaryStatus.Draining:
                    case RollingCertificationSummaryStatus.Stopped:
                        return true;
                }
                return false;
            case RollingCertificationSummaryStatus.Draining:
                return next == RollingCertificationSummaryStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(RollingCertificationSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling certification summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double EffectiveExposure()
    {
        return Math.Round(Score * Rate, 2);
    }

    public RollingCertificationSummary Copy()
    {
        return new RollingCertificationSummary(Code, Name, Tags, Notes, JobCode, Rate, Score, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
