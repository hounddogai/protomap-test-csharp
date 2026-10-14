namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey variance is in its life cycle.</summary>
public enum PulseSurveyVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pulse survey variance. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyVariance(string code, string name, double rate, string source, double threshold, double maximum, string currency, List<string> tags, string costCenter, PulseSurveyVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public double Maximum { get; set; } = maximum;
    public string Currency { get; set; } = currency;
    public List<string> Tags { get; set; } = tags;
    public string CostCenter { get; set; } = costCenter;
    public PulseSurveyVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey variance in status draft.</summary>
    public static PulseSurveyVariance NewPulseSurveyVariance(string code, string name)
    {
        return new PulseSurveyVariance(
            code,
            name,
            0.0,
            "",
            0.0,
            0.0,
            "",
            new List<string>(),
            "",
            PulseSurveyVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyVarianceStatus next)
    {
        switch (Status)
        {
            case PulseSurveyVarianceStatus.Draft:
                return next == PulseSurveyVarianceStatus.Pending || next == PulseSurveyVarianceStatus.Closed;
            case PulseSurveyVarianceStatus.Pending:
                return next == PulseSurveyVarianceStatus.Active || next == PulseSurveyVarianceStatus.Suspended;
            case PulseSurveyVarianceStatus.Active:
                return next == PulseSurveyVarianceStatus.Suspended || next == PulseSurveyVarianceStatus.Closed;
            case PulseSurveyVarianceStatus.Suspended:
                switch (next)
                {
                    case PulseSurveyVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pulse survey variance"} {Code}";
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

    public double AdjustedVolume()
    {
        return Math.Round(Rate - Maximum, 2);
    }

    public PulseSurveyVariance Copy()
    {
        return new PulseSurveyVariance(
            Code,
            Name,
            Rate,
            Source,
            Threshold,
            Maximum,
            Currency,
            Tags,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
