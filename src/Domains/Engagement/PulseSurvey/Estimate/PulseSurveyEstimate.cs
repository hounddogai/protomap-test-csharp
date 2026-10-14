namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey estimate is in its life cycle.</summary>
public enum PulseSurveyEstimateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pulse survey estimate and its current status.</summary>
public sealed class PulseSurveyEstimate(string code, string name, int batchSize, string ownerId, string country, int version, bool active, PulseSurveyEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public string OwnerId { get; set; } = ownerId;
    public string Country { get; set; } = country;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public PulseSurveyEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey estimate in status draft.</summary>
    public static PulseSurveyEstimate NewPulseSurveyEstimate(string code, string name)
    {
        return new PulseSurveyEstimate(code, name, 0, "", "", 0, false, PulseSurveyEstimateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyEstimateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyEstimateStatus next)
    {
        switch (Status)
        {
            case PulseSurveyEstimateStatus.Draft:
                switch (next)
                {
                    case PulseSurveyEstimateStatus.Pending:
                        return true;
                }
                return false;
            case PulseSurveyEstimateStatus.Pending:
                return next == PulseSurveyEstimateStatus.Closed;
            case PulseSurveyEstimateStatus.Active:
                return next == PulseSurveyEstimateStatus.Suspended || next == PulseSurveyEstimateStatus.Closed;
            case PulseSurveyEstimateStatus.Suspended:
                return next == PulseSurveyEstimateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(PulseSurveyEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"pulse survey estimate"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version - (double)BatchSize, 2);
    }

    public PulseSurveyEstimate Copy()
    {
        return new PulseSurveyEstimate(Code, Name, BatchSize, OwnerId, Country, Version, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
