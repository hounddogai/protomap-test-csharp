namespace Acme.Workforce.Engagement.PulseSurvey.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey mapping is in its life cycle.</summary>
public enum PulseSurveyMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pulse survey mapping. Its code is unique within a tenant.</summary>
public sealed class PulseSurveyMapping(string code, string label, int windowDays, double maximum, int endsAt, int sequence, double cost, int retries, int priority, PulseSurveyMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public double Maximum { get; set; } = maximum;
    public int EndsAt { get; set; } = endsAt;
    public int Sequence { get; set; } = sequence;
    public double Cost { get; set; } = cost;
    public int Retries { get; set; } = retries;
    public int Priority { get; set; } = priority;
    public PulseSurveyMappingStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey mapping in status new.</summary>
    public static PulseSurveyMapping NewPulseSurveyMapping(string code, string label)
    {
        return new PulseSurveyMapping(code, label, 0, 0.0, 0, 0, 0.0, 0, 0, PulseSurveyMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyMappingStatus next)
    {
        switch (Status)
        {
            case PulseSurveyMappingStatus.New:
                return next == PulseSurveyMappingStatus.Queued || next == PulseSurveyMappingStatus.Done;
            case PulseSurveyMappingStatus.Queued:
                return next == PulseSurveyMappingStatus.Running;
            case PulseSurveyMappingStatus.Running:
                switch (next)
                {
                    case PulseSurveyMappingStatus.Done:
                    case PulseSurveyMappingStatus.Failed:
                        return true;
                }
                return false;
            case PulseSurveyMappingStatus.Done:
                switch (next)
                {
                    case PulseSurveyMappingStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)EndsAt - (double)Sequence, 2);
    }

    public PulseSurveyMapping Copy()
    {
        return new PulseSurveyMapping(
            Code,
            Label,
            WindowDays,
            Maximum,
            EndsAt,
            Sequence,
            Cost,
            Retries,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
