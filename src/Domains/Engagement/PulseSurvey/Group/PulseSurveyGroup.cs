namespace Acme.Workforce.Engagement.PulseSurvey.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey group is in its life cycle.</summary>
public enum PulseSurveyGroupStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A pulse survey group as the pulse survey team keeps it.</summary>
public sealed class PulseSurveyGroup(string code, string label, double amount, string category, string checksum, bool approved, double weight, double minimum, int windowDays, string segment, double threshold, PulseSurveyGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public string Checksum { get; set; } = checksum;
    public bool Approved { get; set; } = approved;
    public double Weight { get; set; } = weight;
    public double Minimum { get; set; } = minimum;
    public int WindowDays { get; set; } = windowDays;
    public string Segment { get; set; } = segment;
    public double Threshold { get; set; } = threshold;
    public PulseSurveyGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey group in status draft.</summary>
    public static PulseSurveyGroup NewPulseSurveyGroup(string code, string label)
    {
        return new PulseSurveyGroup(
            code,
            label,
            0.0,
            "",
            "",
            false,
            0.0,
            0.0,
            0,
            "",
            0.0,
            PulseSurveyGroupStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyGroupStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyGroupStatus next)
    {
        switch (Status)
        {
            case PulseSurveyGroupStatus.Draft:
                switch (next)
                {
                    case PulseSurveyGroupStatus.Suspended:
                        return true;
                }
                return false;
            case PulseSurveyGroupStatus.Pending:
                return next == PulseSurveyGroupStatus.Active;
            case PulseSurveyGroupStatus.Active:
                switch (next)
                {
                    case PulseSurveyGroupStatus.Suspended:
                    case PulseSurveyGroupStatus.Closed:
                        return true;
                }
                return false;
            case PulseSurveyGroupStatus.Suspended:
                switch (next)
                {
                    case PulseSurveyGroupStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pulse survey group"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round(Threshold + (double)WindowDays, 2);
    }

    public PulseSurveyGroup Copy()
    {
        return new PulseSurveyGroup(
            Code,
            Label,
            Amount,
            Category,
            Checksum,
            Approved,
            Weight,
            Minimum,
            WindowDays,
            Segment,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
